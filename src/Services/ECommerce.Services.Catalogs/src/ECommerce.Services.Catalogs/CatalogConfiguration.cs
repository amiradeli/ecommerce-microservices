using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products;
using ECommerce.Services.Catalogs.Shared.Infrastructure.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Catalogs.Shared.Infrastructure.Extensions.ServiceCollectionExtensions;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs;

public static class CatalogConfiguration
{
    public const string CatalogModulePrefixUri = "api/v1/catalog";

    public static WebApplicationBuilder AddCatalogServices(this WebApplicationBuilder builder)
    {
        AddCatalogServices(builder.Services, builder.Configuration);

        return builder;
    }

    public static IServiceCollection AddCatalogServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddStorage(configuration);

        services.AddCategoriesServices()
            .AddProductsServices()
            .AddSuppliersServices()
            .AddBrandsServices();

        return services;
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", () => "ECommerce.Services.Catalogs Service Apis").ExcludeFromDescription();
        endpoints.MapProductsEndpoints();

        return endpoints;
    }

    public static async Task ConfigureCatalog(
        this IApplicationBuilder app,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        app.UseInfrastructure();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }
}