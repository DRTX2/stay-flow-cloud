using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StayFlow.Persistence;

namespace StayFlow.Api.Observability;

/// <summary>
/// Wires distributed tracing, metrics and health checks. Tracing/metrics are always collected and
/// exposed for Prometheus scraping; an OTLP exporter (Tempo/Jaeger/collector) is added only when an
/// endpoint is configured. Datastore health checks are registered only for the stores that are
/// actually configured, mirroring the optional Redis/Mongo wiring elsewhere.
/// </summary>
public static class ObservabilityExtensions
{
    public const string ServiceName = "StayFlow.Api";

    private static readonly string ServiceVersion =
        typeof(ObservabilityExtensions).Assembly.GetName().Version?.ToString() ?? "1.0.0";

    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var postgres = configuration["STAYFLOW_APP_CONNECTION"] ?? configuration.GetConnectionString("Default");
        postgres = string.IsNullOrWhiteSpace(postgres) ? postgres : PostgreSqlConnectionString.Normalize(postgres);
        var redis = configuration.GetConnectionString("Redis");
        var mongo = configuration.GetConnectionString("Mongo");
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(ServiceName, serviceVersion: ServiceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
                }
            })
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter());

        var healthChecks = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(postgres))
        {
            healthChecks.AddNpgSql(postgres, name: "postgres", tags: ["ready"]);
        }

        if (!string.IsNullOrWhiteSpace(redis))
        {
            healthChecks.AddRedis(redis, name: "redis", tags: ["ready"]);
        }

        if (!string.IsNullOrWhiteSpace(mongo))
        {
            // Reuse the IMongoDatabase registered by AddAudit so we don't open a second client.
            healthChecks.AddMongoDb(
                sp => sp.GetRequiredService<IMongoDatabase>(),
                name: "mongodb",
                tags: ["ready"]);
        }

        return services;
    }

    /// <summary>
    /// Maps the Prometheus scrape endpoint plus liveness/readiness probes. None require auth or
    /// rate limiting so orchestrators and Prometheus can poll them freely.
    /// </summary>
    public static WebApplication MapObservability(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint().DisableRateLimiting();

        // Liveness: process is up and the host is responding. No dependency checks.
        app.MapHealthChecks("/health/live", new()
        {
            Predicate = _ => false,
        }).DisableRateLimiting();

        // Readiness: dependencies (Postgres/Redis/Mongo) are reachable.
        app.MapHealthChecks("/health/ready", new()
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = WriteHealthResponseAsync,
        }).DisableRateLimiting();

        return app;
    }

    private static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = entry.Value.Duration.TotalMilliseconds,
                error = entry.Value.Exception?.Message,
            }),
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
