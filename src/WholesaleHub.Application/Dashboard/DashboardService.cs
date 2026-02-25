using WholesaleHub.Application.Agreements;

namespace WholesaleHub.Application.Dashboard;

public sealed record DashboardDto(
    int CustomerCount,
    int ActiveProductCount,
    int ActiveAgreementCount,
    int DraftAgreementCount,
    int RenewalsDueCount,
    int RenewalWindowDays,
    decimal AnnualAgreementValue);

public interface IDashboardService
{
    Task<DashboardDto> GetAsync(CancellationToken ct);
}

public sealed class DashboardService(IAgreementQueries queries, TimeProvider clock) : IDashboardService
{
    public Task<DashboardDto> GetAsync(CancellationToken ct) =>
        queries.GetDashboardAsync(DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime), AgreementService.DefaultRenewalWindowDays, ct);
}
