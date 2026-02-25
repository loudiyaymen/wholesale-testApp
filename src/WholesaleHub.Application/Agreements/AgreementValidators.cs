using FluentValidation;
using WholesaleHub.Domain.Agreements;

namespace WholesaleHub.Application.Agreements;

public sealed class CreateAgreementRequestValidator : AbstractValidator<CreateAgreementRequest>
{
    public CreateAgreementRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.UnitPrice).GreaterThan(0).PrecisionScale(10, 2, true);
        RuleFor(x => x.TermMonths).InclusiveBetween(PriceAgreement.MinTermMonths, PriceAgreement.MaxTermMonths);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.AnnualVolume).GreaterThan(0).LessThanOrEqualTo(10_000_000);
        // price vs list price is checked in the domain since it needs the product
    }
}

public sealed class CancelAgreementRequestValidator : AbstractValidator<CancelAgreementRequest>
{
    public CancelAgreementRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
