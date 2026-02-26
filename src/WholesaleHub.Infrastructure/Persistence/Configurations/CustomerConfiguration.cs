using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WholesaleHub.Domain.Customers;

namespace WholesaleHub.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.BusinessNumber).HasMaxLength(9).IsFixedLength().IsRequired();
        builder.Property(c => c.ContactName).HasMaxLength(120).IsRequired();
        builder.Property(c => c.ContactEmail).HasMaxLength(254).IsRequired();
        builder.Property(c => c.ContactPhone).HasMaxLength(30);
        builder.Property(c => c.ShippingAddress).HasMaxLength(300).IsRequired();
        builder.Property(c => c.PaymentTerms).HasConversion<string>().HasMaxLength(10);

        builder.HasIndex(c => c.BusinessNumber).IsUnique();
        builder.HasIndex(c => c.LegalName);

        // shadow property so the domain class doesn't need to know about it
        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
