using Duende.IdentityServer.Services;
using Identity.Share.Core;
using Identity.Share.Core.Models;
using Identity.Share.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

// Ref:https://www.scottbrady91.com/identity-server/getting-started-with-identityserver-4
namespace Identity.Share.Infrastructure.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddCustomIdentityServer(this WebApplicationBuilder builder)
    {
        AddCustomIdentityServer(builder.Services);

        return builder;
    }

    public static IServiceCollection AddCustomIdentityServer(this IServiceCollection services)
    {
        services.AddScoped<IProfileService, IdentityProfileService>();

        services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddDeveloperSigningCredential() // This is for dev only scenarios when you don’t have a certificate to use.
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<IdentityProfileService>();

        return services;
    }
}