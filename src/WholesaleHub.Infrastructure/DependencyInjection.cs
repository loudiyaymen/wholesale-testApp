using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WholesaleHub.Application.Abstractions;
using WholesaleHub.Application.Agreements;
using WholesaleHub.Infrastructure.Persistence;
using WholesaleHub.Infrastructure.Persistence.Queries;
using WholesaleHub.Infrastructure.Persistence.Repositories;

namespace WholesaleHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WholesaleHub")
            ?? throw new InvalidOperationException("Connection string 'WholesaleHub' is missing.");

        services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(3)));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IAgreementRepository, AgreementRepository>();
        services.AddScoped<IAgreementQueries, AgreementQueries>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
