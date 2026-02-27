using Microsoft.EntityFrameworkCore;
using WholesaleHub.Domain.Agreements;
using WholesaleHub.Domain.Customers;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.Infrastructure.Persistence;

// fake data for local dev so the screens aren't empty. never runs in prod (see appsettings)
public static class DevDataSeeder
{
    public static async Task SeedAsync(AppDbContext db, TimeProvider clock, CancellationToken ct = default)
    {
        if (await db.Customers.AnyAsync(ct))
            return;

        var now = clock.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        var cornerMarket = Customer.Create("Corner Fresh Market Inc.", "123456789", "Priya Shah",
            "priya@cornerfresh.example", "416-555-0101", "120 Queen St W, Toronto, ON", PaymentTerms.Net30, now);
        var bayview = Customer.Create("Bayview Café Group", "987654321", "Marc Tremblay",
            "marc@bayviewcafe.example", "905-555-0144", "45 Bayview Ave, Toronto, ON", PaymentTerms.Net15, now);
        var northside = Customer.Create("Northside Hardware Ltd.", "456123789", "Amy Chen",
            "amy@northsidehw.example", null, "88 King St E, Hamilton, ON", PaymentTerms.Net60, now);
        db.Customers.AddRange(cornerMarket, bayview, northside);

        var coffee = Product.Create("COF-ARB-1KG", "Arabica beans, medium roast, 1 kg", "Coffee & tea", UnitOfMeasure.Kilogram, 24.50m, now);
        var cups = Product.Create("CUP-12OZ-1000", "Paper cups 12 oz, case of 1000", "Packaging", UnitOfMeasure.Case, 89.00m, now);
        var oil = Product.Create("OIL-CAN-4L", "Canola oil, 4 L jug", "Pantry", UnitOfMeasure.Each, 14.25m, now);
        var screws = Product.Create("SCR-DECK-PAL", "Deck screws #8, pallet of 48 boxes", "Fasteners", UnitOfMeasure.Pallet, 1150.00m, now);
        var gloves = Product.Create("GLV-NIT-L", "Nitrile gloves, large, case of 1000", "Safety", UnitOfMeasure.Case, 64.00m, now);
        var oldMugs = Product.Create("MUG-LOGO-OLD", "Branded mugs (old logo)", "Packaging", UnitOfMeasure.Case, 120.00m, now);
        oldMugs.Discontinue();
        db.Products.AddRange(coffee, cups, oil, screws, gloves, oldMugs);

        // past agreements: pretend "now" was earlier so the domain rules still pass
        db.Agreements.AddRange(
            Signed(bayview.Id, coffee, 21.00m, 12, today.AddMonths(-12).AddDays(9), 2_400, now),    // ends in ~9 days, shows as urgent
            Signed(bayview.Id, cups, 79.00m, 24, today.AddMonths(-6), 300, now),
            Signed(cornerMarket.Id, oil, 12.40m, 12, today.AddMonths(-10), 1_800, now),          // ends in ~2 months
            Signed(northside.Id, screws, 990.00m, 36, today.AddMonths(-20), 40, now),
            PriceAgreement.CreateDraft(northside.Id, gloves, 55.00m, 12, today.AddDays(14), 120, now));

        await db.SaveChangesAsync(ct);
    }

    private static PriceAgreement Signed(Guid customerId, Product product, decimal price, int term, DateOnly start, int volume, DateTimeOffset now)
    {
        var signedAt = now.AddDays(start.DayNumber - DateOnly.FromDateTime(now.UtcDateTime).DayNumber - 5);
        var agreement = PriceAgreement.CreateDraft(customerId, product, price, term, start, volume, signedAt);
        agreement.Activate(signedAt);
        return agreement;
    }
}
