using FluentValidation;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using WholesaleHub.Application.Abstractions;
using WholesaleHub.Application.Common;
using WholesaleHub.Application.Customers;
using WholesaleHub.Application.Products;
using WholesaleHub.Domain.Customers;
using WholesaleHub.Domain.Products;
using static WholesaleHub.UnitTests.TestData;

namespace WholesaleHub.UnitTests.Application;

public class CustomerAndProductServiceTests
{
    private readonly ICustomerRepository _customers = Substitute.For<ICustomerRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private static readonly CustomerRequest ValidCustomer =
        new("Acme Foods Ltd.", "123456789", "Jane Doe", "jane@acme.example", null, "1 Main St", PaymentTerms.Net30);

    private CustomerService Customers() =>
        new(_customers, _uow, new CustomerRequestValidator(), new FakeTimeProvider(Now));

    private ProductService Products() =>
        new(_products, _uow, new CreateProductRequestValidator(), new UpdateProductRequestValidator(), new FakeTimeProvider(Now));

    [Fact]
    public async Task Create_customer_saves_it()
    {
        var result = await Customers().CreateAsync(ValidCustomer, default);

        Assert.Equal("Acme Foods Ltd.", result.LegalName);
        _customers.Received(1).Add(Arg.Is<Customer>(c => c.BusinessNumber == "123456789"));
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Duplicate_business_number_is_a_conflict()
    {
        _customers.BusinessNumberExistsAsync("123456789", null, Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() => Customers().CreateAsync(ValidCustomer, default));
    }

    [Fact]
    public async Task Invalid_customer_reports_each_field()
    {
        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            Customers().CreateAsync(ValidCustomer with { LegalName = "", BusinessNumber = "12", ContactEmail = "nope" }, default));

        var fields = ex.Errors.Select(e => e.PropertyName).Distinct().Order().ToArray();
        Assert.Equal(["BusinessNumber", "ContactEmail", "LegalName"], fields);
    }

    [Fact]
    public async Task Duplicate_sku_is_a_conflict_even_with_different_case()
    {
        _products.SkuExistsAsync("ABC-1", Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            Products().CreateAsync(new CreateProductRequest("abc-1", "Thing", "Stuff", UnitOfMeasure.Each, 5m), default));
    }

    [Fact]
    public async Task Discontinue_product()
    {
        var product = Product();
        _products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var dto = await Products().SetActiveAsync(product.Id, false, default);

        Assert.False(dto.IsActive);
        await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Search_clamps_paging()
    {
        _customers.SearchAsync(null, 1, 100, Arg.Any<CancellationToken>()).Returns(new PagedResult<Customer>([], 1, 100, 0));

        var result = await Customers().SearchAsync(null, -3, 9999, default);

        Assert.Equal(1, result.Page);
        Assert.Equal(100, result.PageSize);
    }
}
