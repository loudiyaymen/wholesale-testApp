using System.Net;
using System.Net.Http.Json;
using WholesaleHub.Application.Agreements;
using WholesaleHub.Application.Common;
using WholesaleHub.Application.Dashboard;
using WholesaleHub.Domain.Agreements;
using static WholesaleHub.IntegrationTests.Helpers;

namespace WholesaleHub.IntegrationTests;

[Collection("api")]
public sealed class AgreementsApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Quote_sign_cancel()
    {
        var customer = await _client.CreateCustomerAsync();
        var product = await _client.CreateProductAsync(listPrice: 50m);

        var created = await _client.QuoteAsync(customer.Id, product.Id, Today.AddDays(7), termMonths: 12, price: 42.50m, volume: 400);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var draft = await created.ReadAsync<AgreementDto>();
        Assert.Equal(AgreementStatus.Draft, draft.Status);
        Assert.Equal(17_000m, draft.AnnualValue);
        Assert.Equal(15m, draft.DiscountPercent);
        Assert.Equal(product.Sku, draft.Sku);

        var signed = await (await _client.PostAsync($"/api/agreements/{draft.Id}/sign", null)).ReadAsync<AgreementDto>();
        Assert.Equal(AgreementStatus.Active, signed.Status);
        Assert.NotNull(signed.SignedAt);

        var cancelled = await (await _client.PostAsJsonAsync($"/api/agreements/{draft.Id}/cancel",
            new CancelAgreementRequest("Customer switched supplier"), Json)).ReadAsync<AgreementDto>();
        Assert.Equal(AgreementStatus.Cancelled, cancelled.Status);
        Assert.Equal("Customer switched supplier", cancelled.CancellationReason);
    }

    [Fact]
    public async Task Signing_an_overlapping_agreement_is_409()
    {
        var customer = await _client.CreateCustomerAsync();
        var product = await _client.CreateProductAsync();
        var first = await (await _client.QuoteAsync(customer.Id, product.Id, Today.AddDays(1), 24)).ReadAsync<AgreementDto>();
        var second = await (await _client.QuoteAsync(customer.Id, product.Id, Today.AddMonths(6), 12)).ReadAsync<AgreementDto>();
        (await _client.PostAsync($"/api/agreements/{first.Id}/sign", null)).EnsureSuccessStatusCode();

        var response = await _client.PostAsync($"/api/agreements/{second.Id}/sign", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Same_product_different_customer_is_fine()
    {
        var a = await _client.CreateCustomerAsync();
        var b = await _client.CreateCustomerAsync();
        var product = await _client.CreateProductAsync();
        var first = await (await _client.QuoteAsync(a.Id, product.Id, Today.AddDays(1))).ReadAsync<AgreementDto>();
        var second = await (await _client.QuoteAsync(b.Id, product.Id, Today.AddDays(1))).ReadAsync<AgreementDto>();

        (await _client.PostAsync($"/api/agreements/{first.Id}/sign", null)).EnsureSuccessStatusCode();
        (await _client.PostAsync($"/api/agreements/{second.Id}/sign", null)).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Too_big_a_discount_is_422()
    {
        var customer = await _client.CreateCustomerAsync();
        var product = await _client.CreateProductAsync(listPrice: 100m);

        var response = await _client.QuoteAsync(customer.Id, product.Id, Today.AddDays(1), price: 50m);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task List_filters_by_customer_and_status()
    {
        var customer = await _client.CreateCustomerAsync();
        var product = await _client.CreateProductAsync();
        var active = await (await _client.QuoteAsync(customer.Id, product.Id, Today.AddDays(1), 12)).ReadAsync<AgreementDto>();
        await _client.QuoteAsync(customer.Id, product.Id, Today.AddYears(2), 12);
        (await _client.PostAsync($"/api/agreements/{active.Id}/sign", null)).EnsureSuccessStatusCode();

        var drafts = await _client.GetFromJsonAsync<PagedResult<AgreementDto>>($"/api/agreements?customerId={customer.Id}&status=Draft", Json);
        var actives = await _client.GetFromJsonAsync<PagedResult<AgreementDto>>($"/api/agreements?customerId={customer.Id}&status=Active", Json);

        Assert.Equal(1, drafts!.TotalCount);
        Assert.Equal(active.Id, Assert.Single(actives!.Items).Id);
    }

    [Fact]
    public async Task Renewals_only_include_agreements_ending_in_the_window()
    {
        var customer = await _client.CreateCustomerAsync();
        var product = await _client.CreateProductAsync();
        var a = await (await _client.QuoteAsync(customer.Id, product.Id, Today, termMonths: 3)).ReadAsync<AgreementDto>();
        (await _client.PostAsync($"/api/agreements/{a.Id}/sign", null)).EnsureSuccessStatusCode();

        var within = await _client.GetFromJsonAsync<List<RenewalDto>>("/api/agreements/renewals?withinDays=120", Json);
        var outside = await _client.GetFromJsonAsync<List<RenewalDto>>("/api/agreements/renewals?withinDays=10", Json);

        var renewal = Assert.Single(within!, r => r.AgreementId == a.Id);
        Assert.Equal(a.EndDate.DayNumber - Today.DayNumber, renewal.DaysRemaining);
        Assert.DoesNotContain(outside!, r => r.AgreementId == a.Id);
    }

    [Fact]
    public async Task Dashboard_and_health()
    {
        var dashboard = await _client.GetFromJsonAsync<DashboardDto>("/api/dashboard", Json);

        Assert.Equal(60, dashboard!.RenewalWindowDays);
        Assert.Equal("Healthy", await _client.GetStringAsync("/health"));
    }
}
