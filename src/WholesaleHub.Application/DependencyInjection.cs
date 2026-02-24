using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using WholesaleHub.Application.Agreements;
using WholesaleHub.Application.Customers;
using WholesaleHub.Application.Dashboard;
using WholesaleHub.Application.Products;

namespace WholesaleHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAgreementService, AgreementService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
