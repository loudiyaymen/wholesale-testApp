using FluentValidation;

namespace WholesaleHub.Application.Products;

public sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(40).Matches("^[A-Za-z0-9-]+$")
            .WithMessage("SKU can only have letters, numbers and dashes.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Unit).IsInEnum();
        RuleFor(x => x.ListPrice).GreaterThan(0).LessThan(1_000_000).PrecisionScale(10, 2, true);
    }
}

// yes this duplicates the rules above, didn't feel worth a shared base class for 3 fields
public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(80);
        RuleFor(x => x.ListPrice).GreaterThan(0).LessThan(1_000_000).PrecisionScale(10, 2, true);
    }
}
