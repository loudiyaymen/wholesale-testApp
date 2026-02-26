using Microsoft.EntityFrameworkCore;
using WholesaleHub.Application.Abstractions;
using WholesaleHub.Application.Common;
using WholesaleHub.Domain.Customers;

namespace WholesaleHub.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(AppDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Customers.SingleOrDefaultAsync(c => c.Id == id, ct);

    public async Task<PagedResult<Customer>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Customers.AsNoTracking();

        // TODO: full text search if this gets slow. LIKE '%x%' can't use the index
        if (!string.IsNullOrEmpty(search))
            query = query.Where(c =>
                c.LegalName.Contains(search) ||
                c.BusinessNumber.Contains(search) ||
                c.ContactName.Contains(search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.LegalName).ThenBy(c => c.Id) // ThenBy so paging is stable with duplicate names
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Customer>(items, page, pageSize, total);
    }

    public Task<bool> BusinessNumberExistsAsync(string businessNumber, Guid? excludingCustomerId, CancellationToken ct) =>
        db.Customers.AnyAsync(c => c.BusinessNumber == businessNumber && c.Id != excludingCustomerId, ct);

    public void Add(Customer customer) => db.Customers.Add(customer);
}
