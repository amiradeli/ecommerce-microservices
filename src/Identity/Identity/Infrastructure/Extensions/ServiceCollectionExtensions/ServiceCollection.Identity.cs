using Ardalis.GuardClauses;
using Identity.Core.Models;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddCustomIdentity(this WebApplicationBuilder builder,
        IConfiguration configuration, Action<IdentityOptions> configure = null)
    {
        AddCustomIdentity(builder.Services, configuration, configure);

        return builder;
    }

    public static IServiceCollection AddCustomIdentity(this IServiceCollection services, IConfiguration configuration,
        Action<IdentityOptions> configure = null)
    {
        //Problem with .net core identity - will override our default authentication scheme `JwtBearerDefaults.AuthenticationScheme` to unwanted `Identity.Application` in `AddIdentity()` method .net identity
        //https://github.com/IdentityServer/IdentityServer4/issues/1525


        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            services.AddDbContext<IdentityContext>(options =>
                options.UseInMemoryDatabase("ShopDB"));
        }
        else
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var connString = configuration.GetConnectionString("ShopDBPostgresConnection");
            Guard.Against.NullOrEmpty(connString, nameof(connString));

            //Postgres
            services.AddDbContext<IdentityContext>(options =>
            {
                options.UseNpgsql(connString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(IdentityContext).Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                }).UseSnakeCaseNamingConvention();
            });
        }

        //https://github.com/IdentityServer/IdentityServer4/issues/1525
        //some dependencies will add here if not registered before
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.RequireUniqueEmail = true;

                if (configure is { })
                    configure.Invoke(options);
            })
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        if (configuration.GetSection(nameof(IdentityOptions)) is not null)
            services.Configure<IdentityOptions>(configuration.GetSection(nameof(IdentityOptions)));

        return services;
    }
}
