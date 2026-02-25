using FluentValidation;

namespace WholesaleHub.Application.Customers;

public sealed class CustomerRequestValidator : AbstractValidator<CustomerRequest>
{
    public CustomerRequestValidator()
    {
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BusinessNumber).NotEmpty().Matches(@"^\d{9}$")
            .WithMessage("Business number must be exactly 9 digits.");
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.ContactPhone).MaximumLength(30); // TODO: actually validate phone format
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(300);
        RuleFor(x => x.PaymentTerms).IsInEnum();
    }
}
