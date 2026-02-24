using WholesaleHub.Domain.Common;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.Domain.Agreements;

// A fixed price for one product for one customer, for a set number of months,
// in exchange for the customer committing to a yearly volume.
public sealed class PriceAgreement : Entity
{
    public const int MinTermMonths = 3;
    public const int MaxTermMonths = 36;

    // sales can't go more than 40% under list without a manager.
    // TODO: manager override instead of a hard block
    public const decimal MaxDiscount = 0.40m;

    public Guid CustomerId { get; private init; }
    public Guid ProductId { get; private init; }

    public decimal UnitPrice { get; private init; }

    // copied from the product when quoting, so the discount % doesn't change if list price moves later
    public decimal ListPriceAtQuote { get; private init; }
    public UnitOfMeasure Unit { get; private init; }

    public int AnnualVolume { get; private init; } // units per year the customer commits to
    public int TermMonths { get; private init; }
    public DateOnly StartDate { get; private init; }
    public DateOnly EndDate { get; private init; }

    public AgreementStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? SignedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public decimal AnnualValue => Math.Round(AnnualVolume * UnitPrice, 2);

    public decimal DiscountPercent => ListPriceAtQuote == 0 ? 0 : Math.Round((1 - UnitPrice / ListPriceAtQuote) * 100, 1);

    private PriceAgreement() { }

    public static PriceAgreement CreateDraft(
        Guid customerId,
        Product product,
        decimal unitPrice,
        int termMonths,
        DateOnly startDate,
        int annualVolume,
        DateTimeOffset now)
    {
        if (!product.IsActive)
            throw new DomainException($"{product.Sku} is discontinued and can't be quoted.");

        if (termMonths is < MinTermMonths or > MaxTermMonths)
            throw new DomainException($"Term must be between {MinTermMonths} and {MaxTermMonths} months.");

        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        // agreements are supposed to be a deal - quoting above list makes no sense
        if (unitPrice > product.ListPrice)
            throw new DomainException($"Unit price can't be above the list price ({product.ListPrice:0.00}).");

        var floor = Math.Round(product.ListPrice * (1 - MaxDiscount), 2);
        if (unitPrice < floor)
            throw new DomainException($"Unit price can't be below {floor:0.00} ({MaxDiscount:P0} off list).");

        if (annualVolume <= 0)
            throw new DomainException("Annual volume must be at least 1.");

        // TODO: this uses UTC "today". A quote made at 9pm in Toronto counts as tomorrow. Fine for now.
        if (startDate < DateOnly.FromDateTime(now.UtcDateTime))
            throw new DomainException("Start date can't be in the past.");

        return new PriceAgreement
        {
            CustomerId = customerId,
            ProductId = product.Id,
            UnitPrice = unitPrice,
            ListPriceAtQuote = product.ListPrice,
            Unit = product.Unit,
            AnnualVolume = annualVolume,
            TermMonths = termMonths,
            StartDate = startDate,
            // whole months, inclusive. 12 months from Jan 1 -> Dec 31, not Jan 1
            EndDate = startDate.AddMonths(termMonths).AddDays(-1),
            Status = AgreementStatus.Draft,
            CreatedAt = now,
        };
    }

    // the caller has to check for overlapping agreements first (needs the db)
    public void Activate(DateTimeOffset now)
    {
        if (Status != AgreementStatus.Draft)
            throw new DomainException($"Only drafts can be signed (this one is {Status}).");

        // edge case: draft sat around so long the whole term is already over
        if (EndDate < DateOnly.FromDateTime(now.UtcDateTime))
            throw new DomainException("This quote's term has already passed. Make a new one.");

        Status = AgreementStatus.Active;
        SignedAt = now;
    }

    public void Cancel(string reason, DateTimeOffset now)
    {
        if (Status == AgreementStatus.Cancelled)
            throw new DomainException("Agreement is already cancelled.");

        if (StatusOn(DateOnly.FromDateTime(now.UtcDateTime)) == AgreementStatus.Expired)
            throw new DomainException("Expired agreements can't be cancelled.");

        CancellationReason = Guard.NotBlank(reason, "Cancellation reason", 500);
        Status = AgreementStatus.Cancelled;
        CancelledAt = now;
    }

    public AgreementStatus StatusOn(DateOnly today) =>
        Status == AgreementStatus.Active && EndDate < today ? AgreementStatus.Expired : Status;

    // true if the two date ranges share at least one day (ranges are inclusive)
    public bool Overlaps(DateOnly otherStart, DateOnly otherEnd) =>
        StartDate <= otherEnd && otherStart <= EndDate;
}
