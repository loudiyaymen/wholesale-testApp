using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MsSql;

namespace WholesaleHub.IntegrationTests;

// Spins up a real SQL Server in docker and runs the API against it.
// Slower than an in-memory db but actually tests the migrations/constraints/SQL. Needs Docker running.
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sql = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:WholesaleHub", _sql.GetConnectionString());
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("Database:SeedSampleData", "false");
    }

    public Task InitializeAsync() => _sql.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await _sql.DisposeAsync();
    }
}

[CollectionDefinition("api")]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>;
