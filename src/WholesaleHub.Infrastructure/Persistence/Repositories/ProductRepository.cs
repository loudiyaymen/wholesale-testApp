using Microsoft.EntityFrameworkCore;
using WholesaleHub.Application.Abstractions;
using WholesaleHub.Application.Common;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(AppDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Products.SingleOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PagedResult<Product>> SearchAsync(string? search, bool includeDiscontinued, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking();

        if (!includeDiscontinued)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Sku.Contains(search) || p.Name.Contains(search) || p.Category.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Category).ThenBy(p => p.Name).ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Product>(items, page, pageSize, total);
    }

    public Task<bool> SkuExistsAsync(string sku, CancellationToken ct) => db.Products.AnyAsync(p => p.Sku == sku, ct);

    public void Add(Product product) => db.Products.Add(product);
}
