using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace Identity.Api.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollection
{
    public static WebApplicationBuilder AddCompression(this WebApplicationBuilder builder)
    {
        AddCompression(builder.Services);

        return builder;
    }

    public static IServiceCollection AddCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        return services;
    }
}
