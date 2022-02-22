using BuildingBlocks.Abstractions.Caching;
using BuildingBlocks.Caching;
using BuildingBlocks.Caching.InMemory;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Extensions.DependencyInjection;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.CQRS;
using BuildingBlocks.Email;
using BuildingBlocks.Logging;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Outbox.EF;
using BuildingBlocks.Messaging.Transport.Rabbitmq;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Scheduling.Internal;
using BuildingBlocks.Validation;
using BuildingBlocks.Web.Extensions;

namespace ECommerce.Services.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
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
        SnowFlakIdGenerator.Configure(2);
        services.AddCore(configuration);

        services.AddMonitoring(healthChecksBuilder =>
        {
            var postgresOptions = configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));
            healthChecksBuilder.AddNpgSql(
                postgresOptions.ConnectionString,
                name: "Customers-Postgres-Check",
                tags: new[] { "customers-postgres" });

            var rabbitMqOptions = configuration.GetOptions<RabbitConfiguration>(nameof(RabbitConfiguration));
            healthChecksBuilder.AddRabbitMQ(
                $"amqp://{rabbitMqOptions.UserName}:{rabbitMqOptions.Password}@{rabbitMqOptions.HostName}{rabbitMqOptions.VirtualHost}",
                name: "CustomersService-RabbitMQ-Check",
                tags: new[] { "customers-rabbitmq" });
        });

        services.AddMessaging(configuration)
            .AddEntityFrameworkOutbox<OutboxDataContext>(configuration, Assembly.GetExecutingAssembly());

        // Or --> Hangfire
        services.AddInternalScheduler<InternalMessageDbContext>(configuration, Assembly.GetExecutingAssembly());

        services.AddRabbitMqTransport(configuration);

        services.AddEmailService(configuration);

        services.AddCqrs(new[] { typeof(CustomersRoot).Assembly }, s =>
        {
            s.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
        });

        services.AddCustomValidators(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCustomInMemoryCache(configuration)
            .AddCachingRequestPolicies( Assembly.GetExecutingAssembly());

        return services;
    }
}
