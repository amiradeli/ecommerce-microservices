using BuildingBlocks.Resiliency;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;

namespace ECommerce.Services.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomHttpClients(
        this IServiceCollection services,
        IConfiguration configuration,
        string pollySectionName = "PolicyConfig")
    {
        services.AddOptions<IdentityApiClientOptions>().Bind(configuration.GetSection(nameof(IdentityApiClientOptions)))
            .ValidateDataAnnotations();

        services.AddOptions<CatalogsApiClientOptions>().Bind(configuration.GetSection(nameof(CatalogsApiClientOptions)))
            .ValidateDataAnnotations();

        services.AddHttpClient<ICatalogApiClient, CatalogApiClient>()
            .AddCustomPolicyHandlers(configuration, pollySectionName);

        services.AddHttpClient<IIdentityApiClient, IdentityApiClient>()
            .AddCustomPolicyHandlers(configuration, pollySectionName);

        return services;
    }
}