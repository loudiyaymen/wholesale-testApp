using FluentValidation;
using WholesaleHub.Application.Abstractions;
using WholesaleHub.Application.Common;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> SearchAsync(string? search, bool includeDiscontinued, int? page, int? pageSize, CancellationToken ct);
    Task<ProductDto> GetAsync(Guid id, CancellationToken ct);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct);
    Task<ProductDto> SetActiveAsync(Guid id, bool active, CancellationToken ct);
}

public sealed class ProductService(
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator,
    TimeProvider clock) : IProductService
{
    public async Task<PagedResult<ProductDto>> SearchAsync(string? search, bool includeDiscontinued, int? page, int? pageSize, CancellationToken ct)
    {
        var (p, size) = Paging.Normalize(page, pageSize);
        var result = await products.SearchAsync(search?.Trim(), includeDiscontinued, p, size, ct);
        return new PagedResult<ProductDto>([.. result.Items.Select(x => x.ToDto())], result.Page, result.PageSize, result.TotalCount);
    }

    public async Task<ProductDto> GetAsync(Guid id, CancellationToken ct) => (await LoadAsync(id, ct)).ToDto();

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        await createValidator.EnsureValidAsync(request, ct);

        var sku = request.Sku.Trim().ToUpperInvariant();
        if (await products.SkuExistsAsync(sku, ct))
            throw new ConflictException($"SKU {sku} already exists.");

        var product = Product.Create(sku, request.Name, request.Category, request.Unit, request.ListPrice, clock.GetUtcNow());
        products.Add(product);
        await unitOfWork.SaveChangesAsync(ct);
        return product.ToDto();
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        await updateValidator.EnsureValidAsync(request, ct);
        var product = await LoadAsync(id, ct);

        // changing list price doesn't touch existing agreements, they keep ListPriceAtQuote
        product.Update(request.Name, request.Category, request.ListPrice);
        await unitOfWork.SaveChangesAsync(ct);
        return product.ToDto();
    }

    public async Task<ProductDto> SetActiveAsync(Guid id, bool active, CancellationToken ct)
    {
        var product = await LoadAsync(id, ct);

        // TODO: warn if there are still active agreements for this product?
        if (active) product.Reinstate();
        else product.Discontinue();

        await unitOfWork.SaveChangesAsync(ct);
        return product.ToDto();
    }

    private async Task<Product> LoadAsync(Guid id, CancellationToken ct) =>
        await products.GetByIdAsync(id, ct) ?? throw new NotFoundException("Product", id);
}
