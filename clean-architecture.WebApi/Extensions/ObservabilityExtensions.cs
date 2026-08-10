using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace clean_architecture.WebApi.Extensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IHostEnvironment env,
        IConfiguration configuration)
    {
        services.AddLogging(logging =>
            logging.AddOpenTelemetry(opts =>
            {
                opts.IncludeFormattedMessage = true;
                opts.IncludeScopes = true;
            })
        );

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: env.ApplicationName,
                    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0"))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithTracing(tracing => tracing
                .AddSource(env.ApplicationName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());

        // OTLP exporter — reads OTEL_EXPORTER_OTLP_ENDPOINT env var automatically.
        // Set to your backend endpoint (Seq, Jaeger, Grafana, Azure Monitor, etc.).
        // Omitting it is safe — telemetry is collected but not exported.
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"]
            ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            services.AddOpenTelemetry().UseOtlpExporter();
        }

        return services;
    }

    /// <summary>
    /// Applies the standard resilience handler (retry, circuit breaker, timeout) to every
    /// <see cref="HttpClient"/> registered via <see cref="IHttpClientFactory"/>, so future outbound
    /// integrations are resilient by default without per-client opt-in.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddHttpResilience(this IServiceCollection services)
    {
        services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());

        return services;
    }
}
