using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", t => t.HasCheckConstraint("CK_Products_ListPrice", "[ListPrice] > 0"));
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Sku).HasMaxLength(40).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Category).HasMaxLength(80).IsRequired();
        builder.Property(p => p.Unit).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ListPrice).HasPrecision(12, 2);

        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Category);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
