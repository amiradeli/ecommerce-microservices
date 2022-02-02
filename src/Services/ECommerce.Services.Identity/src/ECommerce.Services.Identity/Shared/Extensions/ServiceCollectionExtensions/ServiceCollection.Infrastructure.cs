using System.Reflection;
using BuildingBlocks.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Email;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Transport.Rabbitmq;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Validation;
using BuildingBlocks.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Services.Identity.Shared.Extensions.ServiceCollectionExtensions;

public static class ServiceCollection
{
    public static WebApplicationBuilder AddInfrastructure(
        this WebApplicationBuilder builder,
        IConfiguration configuration)
    {
        AddInfrastructure(builder.Services, configuration);

        return builder;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCore();
        services.AddEmailService(configuration);

        services.AddCustomValidators(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCqrs(doMoreActions: s =>
        {
            s.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>));
        });

        services.AddMonitoring(healthChecksBuilder =>
        {
            healthChecksBuilder.AddNpgSql(
                configuration.GetConnectionString("IdentityServiceConnection"),
                name: "Identity-Postgres-Check",
                tags: new[] { "identity-postgres" });

            var rabbitMqOptions = configuration.GetOptions<RabbitConfiguration>(nameof(RabbitConfiguration));

            healthChecksBuilder.AddRabbitMQ(
                $"amqp://{rabbitMqOptions.UserName}:{rabbitMqOptions.Password}@{rabbitMqOptions.HostName}{rabbitMqOptions.VirtualHost}",
                name: "IdentityService-RabbitMQ-Check",
                tags: new[] { "identity-rabbitmq" });
        });

        services.AddMessaging(configuration);
        services.AddRabbitMqTransport(configuration);

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>));

        services.AddCachingRequestPolicies(new List<Assembly> { Assembly.GetExecutingAssembly() });
        services.AddEasyCaching(options => { options.UseInMemory(configuration, "mem"); });

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseMonitoring();

        return app;
    }
}