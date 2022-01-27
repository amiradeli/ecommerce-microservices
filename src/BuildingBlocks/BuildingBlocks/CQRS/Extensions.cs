using System.Reflection;
using BuildingBlocks.Caching;
using BuildingBlocks.CQRS.Command;
using BuildingBlocks.CQRS.Query;
using BuildingBlocks.Logging;
using BuildingBlocks.Validation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CQRS;

public static class Extensions
{
    public static WebApplicationBuilder AddCqrs(
        this WebApplicationBuilder builder,
        Assembly[] assemblies = null,
        Action<IServiceCollection> doMoreActions = null)
    {
        AddCqrs(builder.Services, assemblies, doMoreActions);

        return builder;
    }

    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        Assembly[] assemblies = null,
        Action<IServiceCollection> doMoreActions = null)
    {
        services.AddMediatR(assemblies ?? new[] { Assembly.GetExecutingAssembly() })
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>));

        services.AddTransient<ICommandProcessor, CommandProcessor>()
            .AddTransient<IQueryProcessor, QueryProcessor>();

        return services;
    }
}
