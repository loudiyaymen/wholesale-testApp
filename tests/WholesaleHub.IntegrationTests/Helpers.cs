using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using WholesaleHub.Application.Agreements;
using WholesaleHub.Application.Customers;
using WholesaleHub.Application.Products;
using WholesaleHub.Domain.Customers;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.IntegrationTests;

internal static class Helpers
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    // random values so tests don't trip over each other's unique indexes (they share one db)
    public static string RandomBn() => Random.Shared.Next(100_000_000, 999_999_999).ToString("D9", null);
    public static string RandomSku() => "T-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();

    public static CustomerRequest NewCustomer(string? bn = null) =>
        new("Test Shop Ltd.", bn ?? RandomBn(), "Pat Lee", "pat@test.example", null, "1 Test St, Toronto", PaymentTerms.Net30);

    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>(Json) ?? throw new InvalidOperationException("empty body");

    public static async Task<CustomerDto> CreateCustomerAsync(this HttpClient client)
    {
        var r = await client.PostAsJsonAsync("/api/customers", NewCustomer(), Json);
        r.EnsureSuccessStatusCode();
        return await r.ReadAsync<CustomerDto>();
    }

    public static async Task<ProductDto> CreateProductAsync(this HttpClient client, decimal listPrice = 100m)
    {
        var r = await client.PostAsJsonAsync("/api/products",
            new CreateProductRequest(RandomSku(), "Test widget", "Widgets", UnitOfMeasure.Case, listPrice), Json);
        r.EnsureSuccessStatusCode();
        return await r.ReadAsync<ProductDto>();
    }

    public static Task<HttpResponseMessage> QuoteAsync(this HttpClient client, Guid customerId, Guid productId,
        DateOnly start, int termMonths = 12, decimal price = 90m, int volume = 100) =>
        client.PostAsJsonAsync("/api/agreements", new CreateAgreementRequest(customerId, productId, price, termMonths, start, volume), Json);
}
