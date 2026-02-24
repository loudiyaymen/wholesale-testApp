using WholesaleHub.Application.Common;
using WholesaleHub.Domain.Agreements;
using WholesaleHub.Domain.Customers;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PagedResult<Customer>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct);
    Task<bool> BusinessNumberExistsAsync(string businessNumber, Guid? excludingCustomerId, CancellationToken ct);
    void Add(Customer customer);
}

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PagedResult<Product>> SearchAsync(string? search, bool includeDiscontinued, int page, int pageSize, CancellationToken ct);
    Task<bool> SkuExistsAsync(string sku, CancellationToken ct);
    void Add(Product product);
}

public interface IAgreementRepository
{
    Task<PriceAgreement?> GetByIdAsync(Guid id, CancellationToken ct);

    // all signed agreements for this customer + product, including ones that already ended
    Task<IReadOnlyList<PriceAgreement>> GetActiveAsync(Guid customerId, Guid productId, CancellationToken ct);

    void Add(PriceAgreement agreement);
}

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}
