using FluentValidation;
using WholesaleHub.Application.Abstractions;
using WholesaleHub.Application.Common;
using WholesaleHub.Domain.Agreements;

namespace WholesaleHub.Application.Agreements;

public interface IAgreementService
{
    Task<PagedResult<AgreementDto>> ListAsync(AgreementListQuery query, CancellationToken ct);
    Task<AgreementDto> GetAsync(Guid id, CancellationToken ct);
    Task<AgreementDto> CreateDraftAsync(CreateAgreementRequest request, CancellationToken ct);
    Task<AgreementDto> ActivateAsync(Guid id, CancellationToken ct);
    Task<AgreementDto> CancelAsync(Guid id, CancelAgreementRequest request, CancellationToken ct);
    Task<IReadOnlyList<RenewalDto>> GetRenewalsAsync(int? withinDays, CancellationToken ct);
}

public sealed class AgreementService(
    IAgreementRepository agreements,
    ICustomerRepository customers,
    IProductRepository products,
    IAgreementQueries queries,
    IUnitOfWork unitOfWork,
    IValidator<CreateAgreementRequest> createValidator,
    IValidator<CancelAgreementRequest> cancelValidator,
    TimeProvider clock) : IAgreementService
{
    public const int DefaultRenewalWindowDays = 60;

    private DateOnly Today => DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);

    public Task<PagedResult<AgreementDto>> ListAsync(AgreementListQuery query, CancellationToken ct)
    {
        var (page, pageSize) = Paging.Normalize(query.Page, query.PageSize);
        return queries.ListAsync(query, page, pageSize, Today, ct);
    }

    public async Task<AgreementDto> GetAsync(Guid id, CancellationToken ct) =>
        await queries.GetByIdAsync(id, Today, ct) ?? throw new NotFoundException("Agreement", id);

    public async Task<AgreementDto> CreateDraftAsync(CreateAgreementRequest request, CancellationToken ct)
    {
        await createValidator.EnsureValidAsync(request, ct);

        var customer = await customers.GetByIdAsync(request.CustomerId, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);
        var product = await products.GetByIdAsync(request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        // NOTE: we don't block a draft that overlaps an active agreement - sales quote renewals
        // early all the time. The overlap check happens on activate.
        var agreement = PriceAgreement.CreateDraft(customer.Id, product, request.UnitPrice,
            request.TermMonths, request.StartDate, request.AnnualVolume, clock.GetUtcNow());

        agreements.Add(agreement);
        await unitOfWork.SaveChangesAsync(ct);
        return await GetAsync(agreement.Id, ct);
    }

    public async Task<AgreementDto> ActivateAsync(Guid id, CancellationToken ct)
    {
        var agreement = await LoadAsync(id, ct);

        // one price per customer per product at a time, otherwise invoicing doesn't know which to use
        // FIXME: two people signing two overlapping drafts at the exact same moment would both pass
        // this check. Needs a serializable transaction or an app lock. Very unlikely for now.
        var signed = await agreements.GetActiveAsync(agreement.CustomerId, agreement.ProductId, ct);
        var clash = signed.FirstOrDefault(a => a.Id != agreement.Id && a.Overlaps(agreement.StartDate, agreement.EndDate));
        if (clash is not null)
            throw new ConflictException(
                $"This customer already has a signed price for this product from {clash.StartDate:yyyy-MM-dd} to {clash.EndDate:yyyy-MM-dd}.");

        agreement.Activate(clock.GetUtcNow());
        await unitOfWork.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public async Task<AgreementDto> CancelAsync(Guid id, CancelAgreementRequest request, CancellationToken ct)
    {
        await cancelValidator.EnsureValidAsync(request, ct);
        var agreement = await LoadAsync(id, ct);

        agreement.Cancel(request.Reason, clock.GetUtcNow());
        await unitOfWork.SaveChangesAsync(ct);
        return await GetAsync(id, ct);
    }

    public Task<IReadOnlyList<RenewalDto>> GetRenewalsAsync(int? withinDays, CancellationToken ct)
    {
        var days = Math.Clamp(withinDays ?? DefaultRenewalWindowDays, 1, 365);
        return queries.GetRenewalsAsync(Today, days, ct);
    }

    private async Task<PriceAgreement> LoadAsync(Guid id, CancellationToken ct) =>
        await agreements.GetByIdAsync(id, ct) ?? throw new NotFoundException("Agreement", id);
}
