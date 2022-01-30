using BuildingBlocks.Core.Domain.Events;
using ECommerce.Services.Catalogs.Products.Data;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct;
using ECommerce.Services.Catalogs.Products.Features.DebitingProductStock;
using ECommerce.Services.Catalogs.Products.Features.GettingProductById;
using ECommerce.Services.Catalogs.Products.Features.GettingProducts;
using ECommerce.Services.Catalogs.Products.Features.GettingProductsView;
using ECommerce.Services.Catalogs.Products.Features.ReplenishingProductStock;

namespace ECommerce.Services.Catalogs.Products;

internal static class ProductsConfigs
{
    public const string Tag = "Product";
    public const string ProductsPrefixUri = $"{CatalogModuleConfiguration.CatalogModulePrefixUri}/products";

    internal static IServiceCollection AddProductsServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder, ProductDataSeeder>();

        // services.AddSingleton<IEventMapper<Product>, ProductEventMapper>();
        services.AddSingleton<IEventMapper, ProductEventMapper>();

        return services;
    }

    internal static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapCreateProductsEndpoint()
            .MapGetProductsEndpoint()
            .MapDebitProductStockEndpoint()
            .MapReplenishProductStockEndpoint()
            .MapGetProductByIdEndpoint()
            .MapGetProductsViewEndpoint();
}
