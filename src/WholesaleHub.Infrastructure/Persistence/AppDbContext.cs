using Microsoft.EntityFrameworkCore;
using WholesaleHub.Domain.Agreements;
using WholesaleHub.Domain.Customers;
using WholesaleHub.Domain.Products;

namespace WholesaleHub.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<PriceAgreement> Agreements => Set<PriceAgreement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
