namespace Catalog.Infrastructure.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static async Task ApplyDatabaseMigrations(this IApplicationBuilder app, ILogger logger)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        if (configuration.GetValue<bool>("UseInMemoryDatabase") == false)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            logger.LogInformation("Updating catalog database...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Updated catalog database");
        }
    }
}
