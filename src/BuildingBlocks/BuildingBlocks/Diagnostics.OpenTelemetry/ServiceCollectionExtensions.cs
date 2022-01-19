﻿using System.Diagnostics;
using BuildingBlocks.Diagnostics.OpenTelemetry.Domain;
using BuildingBlocks.Diagnostics.OpenTelemetry.Mediator;
using BuildingBlocks.Diagnostics.OpenTelemetry.Transports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Diagnostics.OpenTelemetry
{
    public static class ServiceCollectionExtensions
    {
        private const string SectionName = "otel";

        public static IServiceCollection AddInMemoryMessagingTelemetry(this IServiceCollection services)
        {
            DiagnosticListener.AllListeners.Subscribe(listener =>
            {
                if (listener.Name == InMemoryTransportListener.InBoundName ||
                    listener.Name == InMemoryTransportListener.OutBoundName)
                {
                    listener.SubscribeWithAdapter(new InMemoryTransportListener());
                }
            });

            return services;
        }

        public static IServiceCollection AddOTelIntegration(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName = SectionName,
            Action<OpenTelemetryOptions> openTelemetryOptions = null)
        {
            var options = configuration.GetSection(sectionName).Get<OpenTelemetryOptions>();
            services.AddOptions<OpenTelemetryOptions>().Bind(configuration.GetSection(sectionName))
                .ValidateDataAnnotations();

            services.AddOpenTelemetryTracing(builder =>
            {
                openTelemetryOptions?.Invoke(options);
                ConfigureSampler(builder, options);
                ConfigureInstrumentation(builder, options);
                ConfigureExporters(configuration, builder, options);

                if (options.Services is not null)
                {
                    foreach (var service in options.Services)
                    {
                        builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                            .AddService(service)); // "Identity.Api"
                    }
                }
            });

            return services;
        }

        private static void ConfigureExporters(
            IConfiguration configuration,
            TracerProviderBuilder builder,
            OpenTelemetryOptions options)
        {
            builder.AddZipkinExporter(o =>
                {
                    configuration.Bind("OtelZipkin", o);
                    o.Endpoint = options.ZipkinExporterOptions.Endpoint; // "http://localhost:9411/api/v2/spans"
                })
                .AddJaegerExporter(c =>
                {
                    c.AgentHost = options.JaegerExporterOptions.AgentHost; // localhost
                    c.AgentPort = options.JaegerExporterOptions.AgentPort; // 6831
                });
        }

        private static void ConfigureSampler(TracerProviderBuilder builder, OpenTelemetryOptions options)
        {
            if (options.AlwaysOnSampler)
            {
                builder.SetSampler(new AlwaysOnSampler());
                builder.SetSampler(new AlwaysOnSampler());
            }
        }

        private static void ConfigureInstrumentation(
            TracerProviderBuilder builder,
            OpenTelemetryOptions options)
        {
            Sdk.SetDefaultTextMapPropagator(GetPropagator(options));
            builder.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddGrpcClientInstrumentation()
                .AddSqlClientInstrumentation(opt => opt.SetDbStatementForText = true)
                .AddSource(OTelMediatROptions.OTelMediatRName)
                .AddSource(OTelDomainOptions.OTelEventHandlerName)
                .AddSource(OTelTransportOptions.InMemoryConsumerActivityName)
                .AddSource(OTelTransportOptions.InMemoryProducerActivityName);
        }

        private static TextMapPropagator GetPropagator(OpenTelemetryOptions openTelemetryOptions)
        {
            var propagators = new List<TextMapPropagator>
            {
                new TraceContextPropagator(),
                new BaggagePropagator(),
            };

            if (openTelemetryOptions.Istio)
            {
                propagators.Add(new B3Propagator());
            }

            return new CompositeTextMapPropagator(propagators);
        }
    }
}