using WholesaleHub.Domain.Agreements;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.Application.Agreements;

public sealed record CreateAgreementRequest(
    Guid CustomerId,
    Guid ProductId,
    decimal UnitPrice,
    int TermMonths,
    DateOnly StartDate,
    int AnnualVolume);

public sealed record CancelAgreementRequest(string Reason);

public sealed record AgreementListQuery(
    AgreementStatus? Status = null,
    Guid? CustomerId = null,
    Guid? ProductId = null,
    int? Page = null,
    int? PageSize = null);

public sealed record AgreementDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid ProductId,
    string Sku,
    string ProductName,
    UnitOfMeasure Unit,
    decimal UnitPrice,
    decimal ListPriceAtQuote,
    int AnnualVolume,
    int TermMonths,
    DateOnly StartDate,
    DateOnly EndDate,
    AgreementStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SignedAt,
    DateTimeOffset? CancelledAt,
    string? CancellationReason)
{
    // computed here instead of in SQL, SQL Server was returning stuff like 12138.0000000
    public decimal AnnualValue => Math.Round(AnnualVolume * UnitPrice, 2);
    public decimal DiscountPercent => ListPriceAtQuote == 0 ? 0 : Math.Round((1 - UnitPrice / ListPriceAtQuote) * 100, 1);
}

public sealed record RenewalDto(
    Guid AgreementId,
    Guid CustomerId,
    string CustomerName,
    string ContactEmail,
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal UnitPrice,
    int AnnualVolume,
    DateOnly EndDate,
    int DaysRemaining)
{
    public decimal AnnualValue => Math.Round(AnnualVolume * UnitPrice, 2);
}
