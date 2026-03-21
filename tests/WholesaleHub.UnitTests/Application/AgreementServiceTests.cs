using FluentValidation;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WholesaleHub.Application.Abstractions;
using WholesaleHub.Application.Agreements;
using WholesaleHub.Application.Common;
using WholesaleHub.Domain.Agreements;
using WholesaleHub.Domain.Products;
using static WholesaleHub.UnitTests.TestData;

namespace WholesaleHub.UnitTests.Application;

public class AgreementServiceTests
{
    private readonly IAgreementRepository _agreements = Substitute.For<IAgreementRepository>();
    private readonly ICustomerRepository _customers = Substitute.For<ICustomerRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IAgreementQueries _queries = Substitute.For<IAgreementQueries>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly AgreementService _sut;

    public AgreementServiceTests()
    {
        _sut = new AgreementService(_agreements, _customers, _products, _queries, _uow,
            new CreateAgreementRequestValidator(), new CancelAgreementRequestValidator(), new FakeTimeProvider(Now));

        // after saving, the service re-reads the dto. we don't care what's in it here
        _queries.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<DateOnly>(), Arg.Any<CancellationToken>())
            .Returns(ci => FakeDto(ci.Arg<Guid>()));
    }

    [Fact]
    public async Task CreateDraft_saves_a_draft()
    {
        var customer = Customer();
        var product = Product(100m);
        _customers.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        _products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        PriceAgreement? added = null;
        _agreements.Add(Arg.Do<PriceAgreement>(a => added = a));

        await _sut.CreateDraftAsync(new CreateAgreementRequest(customer.Id, product.Id, 85m, 12, Today.AddDays(1), 500), default);

        Assert.NotNull(added);
        Assert.Equal(AgreementStatus.Draft, added.Status);
        Assert.Equal(100m, added.ListPriceAtQuote);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateDraft_bad_request_never_hits_the_db()
    {
        var request = new CreateAgreementRequest(Guid.Empty, Guid.Empty, -1m, 99, default, 0);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateDraftAsync(request, default));

        Assert.Contains(ex.Errors, e => e.PropertyName == nameof(CreateAgreementRequest.TermMonths));
        await _customers.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default);
    }

    [Fact]
    public async Task CreateDraft_unknown_product_is_404()
    {
        var customer = Customer();
        _customers.GetByIdAsync(customer.Id, Arg.Any<CancellationToken>()).Returns(customer);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateDraftAsync(new CreateAgreementRequest(customer.Id, Guid.NewGuid(), 1m, 12, Today, 1), default));
    }

    [Fact]
    public async Task Activate_blocks_overlapping_agreement_for_same_customer_and_product()
    {
        var product = Product();
        var customerId = Guid.NewGuid();
        var existing = Draft(product, customerId: customerId, start: Today.AddDays(1), termMonths: 24);
        existing.Activate(Now);
        var draft = Draft(product, customerId: customerId, start: Today.AddMonths(6), termMonths: 12);
        _agreements.GetByIdAsync(draft.Id, Arg.Any<CancellationToken>()).Returns(draft);
        _agreements.GetActiveAsync(customerId, product.Id, Arg.Any<CancellationToken>()).Returns([existing]);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.ActivateAsync(draft.Id, default));

        Assert.Equal(AgreementStatus.Draft, draft.Status);
        await _uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Activate_allows_back_to_back_renewal()
    {
        var product = Product();
        var customerId = Guid.NewGuid();
        var current = Draft(product, customerId: customerId, start: Today.AddDays(1), termMonths: 12);
        current.Activate(Now);
        var renewal = Draft(product, customerId: customerId, start: current.EndDate.AddDays(1), termMonths: 12);
        _agreements.GetByIdAsync(renewal.Id, Arg.Any<CancellationToken>()).Returns(renewal);
        _agreements.GetActiveAsync(customerId, product.Id, Arg.Any<CancellationToken>()).Returns([current]);

        await _sut.ActivateAsync(renewal.Id, default);

        Assert.Equal(AgreementStatus.Active, renewal.Status);
    }

    [Fact]
    public async Task Cancel_unknown_agreement_is_404() =>
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CancelAsync(Guid.NewGuid(), new CancelAgreementRequest("x"), default));

    [Theory]
    [InlineData(null, 60)]
    [InlineData(0, 1)]
    [InlineData(5000, 365)]
    public async Task Renewal_window_is_clamped(int? asked, int expected)
    {
        await _sut.GetRenewalsAsync(asked, default);

        await _queries.Received(1).GetRenewalsAsync(Today, expected, Arg.Any<CancellationToken>());
    }

    private static AgreementDto FakeDto(Guid id) =>
        new(id, Guid.NewGuid(), "Acme", Guid.NewGuid(), "TST-001", "Widget", UnitOfMeasure.Case, 90m, 100m, 1, 12,
            Today, Today.AddYears(1), AgreementStatus.Draft, Now, null, null, null);
}
