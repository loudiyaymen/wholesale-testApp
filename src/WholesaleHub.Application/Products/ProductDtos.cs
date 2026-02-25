using WholesaleHub.Domain.Products;

namespace WholesaleHub.Application.Products;

public sealed record CreateProductRequest(string Sku, string Name, string Category, UnitOfMeasure Unit, decimal ListPrice);

// sku + unit can't change after creation, see Product.Update
public sealed record UpdateProductRequest(string Name, string Category, decimal ListPrice);

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string Category,
    UnitOfMeasure Unit,
    decimal ListPrice,
    bool IsActive);

internal static class ProductMappings
{
    public static ProductDto ToDto(this Product p) => new(p.Id, p.Sku, p.Name, p.Category, p.Unit, p.ListPrice, p.IsActive);
}
