using WholesaleHub.Domain.Common;
using WholesaleHub.Domain.Customers;
using WholesaleHub.Domain.Products;
using static WholesaleHub.UnitTests.TestData;

namespace WholesaleHub.UnitTests.Domain;

public class CustomerAndProductTests
{
    [Fact]
    public void Customer_details_get_trimmed_and_email_lowercased()
    {
        var c = WholesaleHub.Domain.Customers.Customer.Create("  Acme  ", "123456789", " Jane ", " JANE@ACME.EXAMPLE ",
            "  ", " 1 Main St ", PaymentTerms.Net15, Now);

        Assert.Equal("Acme", c.LegalName);
        Assert.Equal("jane@acme.example", c.ContactEmail);
        Assert.Null(c.ContactPhone);
        Assert.Equal("1 Main St", c.ShippingAddress);
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("1234567890")]
    [InlineData("12345678A")]
    // TODO: "123 456 789" too, until we decide whether to strip spaces
    public void Business_number_must_be_9_digits(string bn) =>
        Assert.Throws<DomainException>(() => Customer(bn));

    [Fact]
    public void Sku_is_uppercased() => Assert.Equal("TST-001", Product().Sku);

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void List_price_must_be_positive(decimal price) =>
        Assert.Throws<DomainException>(() => Product(price));

    [Fact]
    public void Discontinue_and_reinstate()
    {
        var p = Product();

        p.Discontinue();
        Assert.False(p.IsActive);

        p.Reinstate();
        Assert.True(p.IsActive);
    }

    [Fact]
    public void Unknown_unit_is_rejected() =>
        Assert.Throws<DomainException>(() =>
            WholesaleHub.Domain.Products.Product.Create("X1", "x", "y", (UnitOfMeasure)99, 1m, Now));
}
