using WholesaleHub.Domain.Common;

namespace WholesaleHub.Domain.Products;

public sealed class Product : Entity
{
    public string Sku { get; private init; } = null!;
    public string Name { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public UnitOfMeasure Unit { get; private init; }

    // our published wholesale price per unit, before any negotiated discount
    public decimal ListPrice { get; private set; }

    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private init; }

    private Product() { }

    public static Product Create(string sku, string name, string category, UnitOfMeasure unit, decimal listPrice, DateTimeOffset now)
    {
        if (!Enum.IsDefined(unit))
            throw new DomainException("Unknown unit of measure.");

        var product = new Product
        {
            Sku = Guard.NotBlank(sku, "SKU", 40).ToUpperInvariant(),
            Unit = unit,
            CreatedAt = now,
        };
        product.Update(name, category, listPrice);
        return product;
    }

    // NOTE: unit is deliberately not editable. Switching "Case" to "Each" would silently
    // change what every existing agreement means. Make a new SKU instead.
    public void Update(string name, string category, decimal listPrice)
    {
        Name = Guard.NotBlank(name, "Name", 200);
        Category = Guard.NotBlank(category, "Category", 80);

        if (listPrice <= 0)
            throw new DomainException("List price must be greater than zero.");
        ListPrice = listPrice;
    }

    // discontinued products stay in the db because old agreements point at them
    public void Discontinue() => IsActive = false;

    public void Reinstate() => IsActive = true;
}
