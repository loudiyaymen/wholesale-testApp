using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesaleHub.Domain.Agreements;
using WholesaleHub.Domain.Customers;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.Infrastructure.Persistence.Configurations;

internal sealed class PriceAgreementConfiguration : IEntityTypeConfiguration<PriceAgreement>
{
    public void Configure(EntityTypeBuilder<PriceAgreement> builder)
    {
        builder.ToTable("PriceAgreements", t =>
        {
            // belt and braces, the domain already checks these
            t.HasCheckConstraint("CK_Agreements_Price", "[UnitPrice] > 0");
            t.HasCheckConstraint("CK_Agreements_Term", "[TermMonths] BETWEEN 3 AND 36");
            t.HasCheckConstraint("CK_Agreements_Volume", "[AnnualVolume] > 0");
            t.HasCheckConstraint("CK_Agreements_Dates", "[EndDate] > [StartDate]");
        });
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.UnitPrice).HasPrecision(12, 2);
        builder.Property(a => a.ListPriceAtQuote).HasPrecision(12, 2);
        builder.Property(a => a.Unit).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);
        builder.Ignore(a => a.AnnualValue);
        builder.Ignore(a => a.DiscountPercent);
        builder.Property<byte[]>("RowVersion").IsRowVersion();

        // no navigation props on purpose, agreements only hold the ids
        builder.HasOne<Customer>().WithMany().HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Product>().WithMany().HasForeignKey(a => a.ProductId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.CustomerId, a.ProductId, a.Status });
        builder.HasIndex(a => new { a.Status, a.EndDate }); // for the renewals query
    }
}
