using BuildingBlocks.Monitoring;

namespace ECommerce.Services.Customers.Shared.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseMonitoring();

        return app;
    }
}