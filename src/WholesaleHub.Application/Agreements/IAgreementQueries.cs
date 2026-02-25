using WholesaleHub.Application.Common;
using WholesaleHub.Application.Dashboard;

namespace WholesaleHub.Application.Agreements;

// Read side. These go straight from the db into DTOs (no tracking, joins done in SQL)
// instead of loading full entities. Writes still go through the repositories.
public interface IAgreementQueries
{
    Task<AgreementDto?> GetByIdAsync(Guid id, DateOnly today, CancellationToken ct);
    Task<PagedResult<AgreementDto>> ListAsync(AgreementListQuery query, int page, int pageSize, DateOnly today, CancellationToken ct);
    Task<IReadOnlyList<RenewalDto>> GetRenewalsAsync(DateOnly today, int withinDays, CancellationToken ct);
    Task<DashboardDto> GetDashboardAsync(DateOnly today, int renewalWindowDays, CancellationToken ct);
}
