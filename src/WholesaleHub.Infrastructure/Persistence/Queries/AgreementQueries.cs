using Microsoft.EntityFrameworkCore;
using WholesaleHub.Application.Agreements;
using WholesaleHub.Application.Common;
using WholesaleHub.Application.Dashboard;
using WholesaleHub.Domain.Agreements;

namespace WholesaleHub.Infrastructure.Persistence.Queries;

internal sealed class AgreementQueries(AppDbContext db) : IAgreementQueries
{
    public Task<AgreementDto?> GetByIdAsync(Guid id, DateOnly today, CancellationToken ct) =>
        Project(db.Agreements.Where(a => a.Id == id), today).SingleOrDefaultAsync(ct);

    public async Task<PagedResult<AgreementDto>> ListAsync(AgreementListQuery query, int page, int pageSize, DateOnly today, CancellationToken ct)
    {
        var agreements = db.Agreements.AsQueryable();

        if (query.CustomerId is { } customerId)
            agreements = agreements.Where(a => a.CustomerId == customerId);
        if (query.ProductId is { } productId)
            agreements = agreements.Where(a => a.ProductId == productId);

        // Expired isn't a stored status so Active/Expired have to look at the end date
        agreements = query.Status switch
        {
            null => agreements,
            AgreementStatus.Active => agreements.Where(a => a.Status == AgreementStatus.Active && a.EndDate >= today),
            AgreementStatus.Expired => agreements.Where(a => a.Status == AgreementStatus.Active && a.EndDate < today),
            var status => agreements.Where(a => a.Status == status),
        };

        var total = await agreements.CountAsync(ct);
        var items = await Project(
                agreements.OrderByDescending(a => a.CreatedAt).ThenBy(a => a.Id).Skip((page - 1) * pageSize).Take(pageSize),
                today)
            .ToListAsync(ct);

        return new PagedResult<AgreementDto>(items, page, pageSize, total);
    }

    public async Task<IReadOnlyList<RenewalDto>> GetRenewalsAsync(DateOnly today, int withinDays, CancellationToken ct)
    {
        var rows = await (
                from a in EndingBetween(today, today.AddDays(withinDays))
                join c in db.Customers on a.CustomerId equals c.Id
                join p in db.Products on a.ProductId equals p.Id
                orderby a.EndDate, c.LegalName
                select new { a.Id, a.CustomerId, c.LegalName, c.ContactEmail, a.ProductId, p.Sku, p.Name, a.UnitPrice, a.AnnualVolume, a.EndDate })
            .AsNoTracking()
            .ToListAsync(ct);

        // days remaining worked out in C#, DateOnly.DayNumber doesn't translate to SQL
        return [.. rows.Select(r => new RenewalDto(r.Id, r.CustomerId, r.LegalName, r.ContactEmail, r.ProductId, r.Sku, r.Name,
            r.UnitPrice, r.AnnualVolume, r.EndDate, r.EndDate.DayNumber - today.DayNumber))];
    }

    public async Task<DashboardDto> GetDashboardAsync(DateOnly today, int renewalWindowDays, CancellationToken ct)
    {
        var active = db.Agreements.Where(a => a.Status == AgreementStatus.Active && a.EndDate >= today);

        // one after another, DbContext can't run queries in parallel
        var customers = await db.Customers.CountAsync(ct);
        var products = await db.Products.CountAsync(p => p.IsActive, ct);
        var activeCount = await active.CountAsync(ct);
        var drafts = await db.Agreements.CountAsync(a => a.Status == AgreementStatus.Draft, ct);
        var renewals = await EndingBetween(today, today.AddDays(renewalWindowDays)).CountAsync(ct);
        var value = await active.SumAsync(a => a.AnnualVolume * a.UnitPrice, ct);

        return new DashboardDto(customers, products, activeCount, drafts, renewals, renewalWindowDays, Math.Round(value, 2));
    }

    private IQueryable<PriceAgreement> EndingBetween(DateOnly from, DateOnly to) =>
        db.Agreements.Where(a => a.Status == AgreementStatus.Active && a.EndDate >= from && a.EndDate <= to);

    private IQueryable<AgreementDto> Project(IQueryable<PriceAgreement> agreements, DateOnly today) =>
        from a in agreements.AsNoTracking()
        join c in db.Customers on a.CustomerId equals c.Id
        join p in db.Products on a.ProductId equals p.Id
        select new AgreementDto(
            a.Id, a.CustomerId, c.LegalName, a.ProductId, p.Sku, p.Name, a.Unit,
            a.UnitPrice, a.ListPriceAtQuote, a.AnnualVolume, a.TermMonths, a.StartDate, a.EndDate,
            a.Status == AgreementStatus.Active && a.EndDate < today ? AgreementStatus.Expired : a.Status,
            a.CreatedAt, a.SignedAt, a.CancelledAt, a.CancellationReason);
}
