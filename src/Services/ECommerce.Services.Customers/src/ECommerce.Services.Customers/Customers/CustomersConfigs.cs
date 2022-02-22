using BuildingBlocks.Abstractions.Domain.Events;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Web.Module;
using ECommerce.Services.Customers.Customers.Data;

namespace ECommerce.Services.Customers.Customers;

internal class CustomersConfigs : IModuleDefinition
{
    public const string Tag = "Customers";
    public const string CustomersPrefixUri = $"{CustomersModuleConfiguration.CustomerModulePrefixUri}";

    public IServiceCollection AddModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDataSeeder, CustomersDataSeeder>();
        services.AddSingleton<IIntegrationEventMapper, CustomersEventMapper>();

        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        return endpoints;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult(app);
    }
}
