using WholesaleHub.Domain.Agreements;
using WholesaleHub.Domain.Customers;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.UnitTests;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
    public static readonly DateOnly Today = DateOnly.FromDateTime(Now.UtcDateTime);

    public static Customer Customer(string bn = "123456789") =>
        WholesaleHub.Domain.Customers.Customer.Create("Acme Foods Ltd.", bn, "Jane Doe", "Jane@Acme.example",
            "416-555-0100", "1 Main St, Toronto", PaymentTerms.Net30, Now);

    public static Product Product(decimal listPrice = 100m) =>
        WholesaleHub.Domain.Products.Product.Create("tst-001", "Test widget", "Widgets", UnitOfMeasure.Case, listPrice, Now);

    public static PriceAgreement Draft(
        Product? product = null,
        decimal price = 90m,
        int termMonths = 12,
        DateOnly? start = null,
        int volume = 100,
        Guid? customerId = null) =>
        PriceAgreement.CreateDraft(customerId ?? Guid.NewGuid(), product ?? Product(), price, termMonths,
            start ?? Today.AddDays(10), volume, Now);
}
