using System.Globalization;
using WholesaleHub.Domain.Agreements;
using WholesaleHub.Domain.Common;
using static WholesaleHub.UnitTests.TestData;

namespace WholesaleHub.UnitTests.Domain;

public class PriceAgreementTests
{
    [Fact]
    public void End_date_is_last_day_of_the_term()
    {
        var agreement = Draft(start: new DateOnly(2026, 4, 1), termMonths: 12);

        Assert.Equal(new DateOnly(2027, 3, 31), agreement.EndDate);
        Assert.Equal(AgreementStatus.Draft, agreement.Status);
    }

    [Fact]
    public void Snapshots_list_price_and_unit_from_product()
    {
        var product = Product(listPrice: 50m);
        var agreement = Draft(product, price: 45m);

        // list price changes later shouldn't affect the agreement
        product.Update(product.Name, product.Category, 80m);

        Assert.Equal(50m, agreement.ListPriceAtQuote);
        Assert.Equal(product.Unit, agreement.Unit);
        Assert.Equal(10m, agreement.DiscountPercent);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(37)]
    public void Rejects_terms_outside_3_to_36_months(int months) =>
        Assert.Throws<DomainException>(() => Draft(termMonths: months));

    [Theory]
    [InlineData(3)]
    [InlineData(36)]
    public void Accepts_boundary_terms(int months) => Assert.Equal(months, Draft(termMonths: months).TermMonths);

    [Fact]
    public void Price_above_list_is_rejected() =>
        Assert.Throws<DomainException>(() => Draft(Product(100m), price: 100.01m));

    [Fact]
    public void Price_equal_to_list_is_ok() => Assert.Equal(0m, Draft(Product(100m), price: 100m).DiscountPercent);

    [Theory]
    [InlineData(59.99, false)] // more than 40% off
    [InlineData(60.00, true)]  // exactly 40% off is the limit
    public void Discount_is_capped_at_40_percent(decimal price, bool allowed)
    {
        var act = () => Draft(Product(100m), price: price);

        if (allowed) act();
        else Assert.Throws<DomainException>(act);
    }

    [Fact]
    public void Cant_quote_a_discontinued_product()
    {
        var product = Product();
        product.Discontinue();

        var ex = Assert.Throws<DomainException>(() => Draft(product));
        Assert.Contains("discontinued", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Start_date_cant_be_in_the_past() =>
        Assert.Throws<DomainException>(() => Draft(start: Today.AddDays(-1)));

    [Fact]
    public void Volume_must_be_positive() => Assert.Throws<DomainException>(() => Draft(volume: 0));

    [Fact]
    public void Annual_value_is_volume_times_price()
    {
        var agreement = Draft(Product(30m), price: 24.99m, volume: 1_234);

        Assert.Equal(30_837.66m, agreement.AnnualValue);
    }

    [Fact]
    public void Activate_signs_a_draft()
    {
        var agreement = Draft();

        agreement.Activate(Now);

        Assert.Equal(AgreementStatus.Active, agreement.Status);
        Assert.Equal(Now, agreement.SignedAt);
    }

    [Fact]
    public void Cant_activate_twice()
    {
        var agreement = Draft();
        agreement.Activate(Now);

        Assert.Throws<DomainException>(() => agreement.Activate(Now));
    }

    [Fact]
    public void Cant_activate_after_term_already_passed()
    {
        var agreement = Draft(termMonths: 3);

        Assert.Throws<DomainException>(() => agreement.Activate(Now.AddYears(1)));
    }

    [Fact]
    public void Cancel_needs_a_reason()
    {
        var agreement = Draft();

        Assert.Throws<DomainException>(() => agreement.Cancel("   ", Now));
        Assert.Equal(AgreementStatus.Draft, agreement.Status);
    }

    [Fact]
    public void Cancel_saves_reason_and_date()
    {
        var agreement = Draft();
        agreement.Activate(Now);

        agreement.Cancel("Store closed", Now.AddDays(2));

        Assert.Equal(AgreementStatus.Cancelled, agreement.Status);
        Assert.Equal("Store closed", agreement.CancellationReason);
        Assert.Equal(Now.AddDays(2), agreement.CancelledAt);
    }

    [Fact]
    public void Cant_cancel_an_expired_agreement()
    {
        var agreement = Draft(termMonths: 3);
        agreement.Activate(Now);

        Assert.Throws<DomainException>(() => agreement.Cancel("too late", Now.AddYears(1)));
    }

    [Fact]
    public void Expired_is_derived_from_end_date()
    {
        var active = Draft(termMonths: 6);
        active.Activate(Now);
        var draft = Draft(termMonths: 6);

        Assert.Equal(AgreementStatus.Active, active.StatusOn(active.EndDate));
        Assert.Equal(AgreementStatus.Expired, active.StatusOn(active.EndDate.AddDays(1)));
        Assert.Equal(AgreementStatus.Draft, draft.StatusOn(draft.EndDate.AddDays(1))); // drafts don't "expire"
    }

    [Theory]
    [InlineData("2026-01-01", "2026-03-31", false)]
    [InlineData("2026-01-01", "2026-04-01", true)] // touches the first day
    [InlineData("2026-06-01", "2026-07-01", true)]
    [InlineData("2027-03-31", "2028-01-01", true)] // touches the last day
    [InlineData("2027-04-01", "2028-01-01", false)]
    public void Overlaps(string start, string end, bool expected)
    {
        var agreement = Draft(start: new DateOnly(2026, 4, 1), termMonths: 12); // 2026-04-01 .. 2027-03-31

        Assert.Equal(expected, agreement.Overlaps(
            DateOnly.Parse(start, CultureInfo.InvariantCulture), DateOnly.Parse(end, CultureInfo.InvariantCulture)));
    }
}
