using Microsoft.EntityFrameworkCore;
using WholesaleHub.Application.Abstractions;
using WholesaleHub.Domain.Agreements;

namespace WholesaleHub.Infrastructure.Persistence.Repositories;

internal sealed class AgreementRepository(AppDbContext db) : IAgreementRepository
{
    public Task<PriceAgreement?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Agreements.SingleOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<PriceAgreement>> GetActiveAsync(Guid customerId, Guid productId, CancellationToken ct) =>
        await db.Agreements
            .Where(a => a.CustomerId == customerId && a.ProductId == productId && a.Status == AgreementStatus.Active)
            .ToListAsync(ct);

    public void Add(PriceAgreement agreement) => db.Agreements.Add(agreement);
}
