using Microsoft.AspNetCore.Mvc;
using WholesaleHub.Application.Agreements;
using WholesaleHub.Application.Common;
using WholesaleHub.Domain.Agreements;

namespace WholesaleHub.Api.Controllers;

[ApiController]
[Route("api/agreements")]
[Produces("application/json")]
public sealed class AgreementsController(IAgreementService agreements) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<AgreementDto>> List(
        [FromQuery] AgreementStatus? status, [FromQuery] Guid? customerId, [FromQuery] Guid? productId,
        [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct) =>
        agreements.ListAsync(new AgreementListQuery(status, customerId, productId, page, pageSize), ct);

    // signed agreements ending in the next N days (default 60)
    [HttpGet("renewals")]
    public Task<IReadOnlyList<RenewalDto>> Renewals([FromQuery] int? withinDays, CancellationToken ct) =>
        agreements.GetRenewalsAsync(withinDays, ct);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AgreementDto>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    public Task<AgreementDto> Get(Guid id, CancellationToken ct) => agreements.GetAsync(id, ct);

    // creates a draft (a quote). Nothing is binding until /sign
    [HttpPost]
    [ProducesResponseType<AgreementDto>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    public async Task<ActionResult<AgreementDto>> Create(CreateAgreementRequest request, CancellationToken ct)
    {
        var created = await agreements.CreateDraftAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/sign")]
    [ProducesResponseType<AgreementDto>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(409)]
    [ProducesResponseType<ProblemDetails>(422)]
    public Task<AgreementDto> Sign(Guid id, CancellationToken ct) => agreements.ActivateAsync(id, ct);

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<AgreementDto>(200)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(422)]
    public Task<AgreementDto> Cancel(Guid id, CancelAgreementRequest request, CancellationToken ct) => agreements.CancelAsync(id, request, ct);
}
