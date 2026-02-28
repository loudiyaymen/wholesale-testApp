using Microsoft.AspNetCore.Mvc;
using WholesaleHub.Application.Common;
using WholesaleHub.Application.Customers;

namespace WholesaleHub.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public sealed class CustomersController(ICustomerService customers) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<CustomerDto>> Search([FromQuery] string? search, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct) =>
        customers.SearchAsync(search, page, pageSize, ct);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<CustomerDto>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    public Task<CustomerDto> Get(Guid id, CancellationToken ct) => customers.GetAsync(id, ct);

    [HttpPost]
    [ProducesResponseType<CustomerDto>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(409)]
    public async Task<ActionResult<CustomerDto>> Create(CustomerRequest request, CancellationToken ct)
    {
        var created = await customers.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CustomerDto>(200)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(404)]
    [ProducesResponseType<ProblemDetails>(409)]
    public Task<CustomerDto> Update(Guid id, CustomerRequest request, CancellationToken ct) => customers.UpdateAsync(id, request, ct);

    // TODO: DELETE. Needs a rule for customers that still have agreements (soft delete probably)
}
