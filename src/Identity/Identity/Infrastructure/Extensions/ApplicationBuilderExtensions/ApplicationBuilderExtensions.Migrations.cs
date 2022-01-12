using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static async Task ApplyDatabaseMigrations(this IApplicationBuilder app, ILogger logger)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        if (configuration.GetValue<bool>("UseInMemoryDatabase") == false)
        {
            using IServiceScope serviceScope = app.ApplicationServices.CreateScope();
            IdentityContext dbContext = serviceScope.ServiceProvider.GetRequiredService<IdentityContext>();
            logger.LogInformation("Updating database...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Updated database");
        }
    }
}
