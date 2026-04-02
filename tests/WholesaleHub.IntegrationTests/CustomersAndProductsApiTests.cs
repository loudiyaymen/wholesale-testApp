using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using WholesaleHub.Application.Common;
using WholesaleHub.Application.Customers;
using WholesaleHub.Application.Products;
using WholesaleHub.Domain.Products;
using static WholesaleHub.IntegrationTests.Helpers;

namespace WholesaleHub.IntegrationTests;

[Collection("api")]
public sealed class CustomersAndProductsApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_customer_returns_201_and_location()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", NewCustomer(), Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<CustomerDto>(response.Headers.Location, Json);
        Assert.Equal("pat@test.example", fetched!.ContactEmail);
    }

    [Fact]
    public async Task Invalid_customer_gives_field_errors()
    {
        var response = await _client.PostAsJsonAsync("/api/customers", NewCustomer() with { BusinessNumber = "12AB", ContactEmail = "x" }, Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.ReadAsync<ValidationProblemDetails>();
        Assert.Contains("businessNumber", problem.Errors.Keys);
        Assert.Contains("contactEmail", problem.Errors.Keys);
    }

    [Fact]
    public async Task Duplicate_business_number_is_409()
    {
        var bn = RandomBn();
        (await _client.PostAsJsonAsync("/api/customers", NewCustomer(bn), Json)).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/customers", NewCustomer(bn), Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Update_customer()
    {
        var c = await _client.CreateCustomerAsync();

        var response = await _client.PutAsJsonAsync($"/api/customers/{c.Id}", NewCustomer(c.BusinessNumber) with { LegalName = "Renamed" }, Json);

        response.EnsureSuccessStatusCode();
        Assert.Equal("Renamed", (await response.ReadAsync<CustomerDto>()).LegalName);
    }

    [Fact]
    public async Task Unknown_customer_is_404() =>
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/customers/{Guid.NewGuid()}")).StatusCode);

    [Fact]
    public async Task Duplicate_sku_is_409()
    {
        var p = await _client.CreateProductAsync();

        var response = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest(p.Sku.ToLowerInvariant(), "Other", "Other", UnitOfMeasure.Each, 1m), Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Discontinued_products_are_hidden_unless_asked_for()
    {
        var p = await _client.CreateProductAsync();
        (await _client.PostAsync($"/api/products/{p.Id}/discontinue", null)).EnsureSuccessStatusCode();

        var hidden = await _client.GetFromJsonAsync<PagedResult<ProductDto>>($"/api/products?search={p.Sku}", Json);
        var shown = await _client.GetFromJsonAsync<PagedResult<ProductDto>>($"/api/products?search={p.Sku}&includeDiscontinued=true", Json);

        Assert.Empty(hidden!.Items);
        Assert.False(Assert.Single(shown!.Items).IsActive);
    }
}
