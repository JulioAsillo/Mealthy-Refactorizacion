using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Defaults compartidos por todos los servicios .NET de la solución (hoy solo Mealthy.Api):
/// OpenTelemetry (logs, métricas, trazas), health check de liveness, service discovery y resiliencia HTTP.
///
/// Diferencia con la plantilla de Aspire: NO se incluye MapDefaultEndpoints(). La API ya expone su
/// propio GET /health (JSON detallado, Shared/Infraestructure/Web/HealthCheckExtensions.cs), y tener
/// los dos endpoints en la misma ruta provoca un AmbiguousMatchException.
/// </summary>
public static class Extensions
{
    /// <summary>Rutas que no se trazan (ruido del sondeo de salud de Aspire).</summary>
    private const string HealthEndpointPath = "/health";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();   // retry + circuit breaker + timeouts (Polly v8)
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var appName = builder.Environment.ApplicationName;   // "Mealthy.Api"

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(appName, $"{appName}.*"))
            .WithTracing(tracing => tracing
                // ActivitySource propios: "Mealthy.Api" y "Mealthy.Api.<Contexto>" (ej. Mealthy.Api.Media)
                .AddSource(appName, $"{appName}.*")
                .AddAspNetCoreInstrumentation(o =>
                    o.Filter = ctx => !ctx.Request.Path.StartsWithSegments(HealthEndpointPath))
                .AddHttpClientInstrumentation());

        // Aspire inyecta OTEL_EXPORTER_OTLP_ENDPOINT → el dashboard recibe la telemetría.
        // Sin Aspire (dotnet run a secas) no se exporta nada y no falla.
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
            builder.Services.AddOpenTelemetry().UseOtlpExporter();

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // "self": el proceso responde. Reemplaza al check "api" que antes registraba la propia API.
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("La API responde."), ["live"]);

        return builder;
    }
}
