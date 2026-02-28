using Microsoft.AspNetCore.Mvc;
using WholesaleHub.Application.Dashboard;

namespace WholesaleHub.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboard) : ControllerBase
{
    [HttpGet]
    public Task<DashboardDto> Get(CancellationToken ct) => dashboard.GetAsync(ct);
}
