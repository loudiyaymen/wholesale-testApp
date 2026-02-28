using Microsoft.AspNetCore.Mvc;
using WholesaleHub.Application.Common;
using WholesaleHub.Application.Products;

namespace WholesaleHub.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public sealed class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<ProductDto>> Search(
        [FromQuery] string? search, [FromQuery] bool includeDiscontinued, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct) =>
        products.SearchAsync(search, includeDiscontinued, page, pageSize, ct);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductDto>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    public Task<ProductDto> Get(Guid id, CancellationToken ct) => products.GetAsync(id, ct);

    [HttpPost]
    [ProducesResponseType<ProductDto>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(409)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var created = await products.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ProductDto>(200)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType<ProblemDetails>(404)]
    public Task<ProductDto> Update(Guid id, UpdateProductRequest request, CancellationToken ct) => products.UpdateAsync(id, request, ct);

    [HttpPost("{id:guid}/discontinue")]
    public Task<ProductDto> Discontinue(Guid id, CancellationToken ct) => products.SetActiveAsync(id, false, ct);

    [HttpPost("{id:guid}/reinstate")]
    public Task<ProductDto> Reinstate(Guid id, CancellationToken ct) => products.SetActiveAsync(id, true, ct);
}
