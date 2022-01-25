using BuildingBlocks.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.CQRS;
using BuildingBlocks.Email;
using BuildingBlocks.IdsGenerator;
using BuildingBlocks.Messaging;
using BuildingBlocks.Messaging.Transport.Rabbitmq;
using BuildingBlocks.Validation;

namespace Catalog.Shared.Infrastructure.Extensions.ServiceCollectionExtensions;

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
        SnowFlakIdGenerator.Configure(1);
        services.AddCore();

        services.AddMessaging(configuration, TxOutboxConstants.EntityFramework);
        services.AddRabbitMqTransport(configuration);

        services.AddEmailService(configuration);
        services.AddCustomMediatR();
        services.AddCqrs();
        services.AddCustomValidators(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCachingRequestPolicies(new List<Assembly> { Assembly.GetExecutingAssembly() });
        services.AddEasyCaching(options => { options.UseInMemory(configuration, "mem"); });

        return services;
    }
}