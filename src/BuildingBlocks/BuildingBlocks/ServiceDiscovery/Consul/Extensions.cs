using BuildingBlocks.Web.Extensions;
using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.ServiceDiscovery.Consul;

public static class Extensions
{
    /// <summary>
    /// Add Consul service discovery on <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configurator">The configurator.</param>
    /// <returns>A ServiceCollection.</returns>
    public static IServiceCollection AddConsulServiceDiscovery(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<ConsulOptions> configurator)
    {
        var consulOptions = configuration.GetOptions<ConsulOptions>("Consul");

        services.Configure<ConsulOptions>(configuration.GetSection(nameof(ConsulOptions)));
        if (configurator is { })
            services.Configure(nameof(ConsulOptions), configurator);

        services.AddSingleton<IHostedService, ConsulServiceDiscoveryHostedService>();

        services.AddSingleton<IConsulClient, ConsulClient>(provider =>
            new ConsulClient(clientConfiguration =>
                clientConfiguration.Address = new Uri(consulOptions.ConsulAddress)));

        return services;
    }
}