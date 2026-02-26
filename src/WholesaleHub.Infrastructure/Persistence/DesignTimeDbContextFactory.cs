using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WholesaleHub.Infrastructure.Persistence;

// only used by `dotnet ef migrations add`, the connection string here doesn't matter
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=WholesaleHub;TrustServerCertificate=True")
            .Options);
}
