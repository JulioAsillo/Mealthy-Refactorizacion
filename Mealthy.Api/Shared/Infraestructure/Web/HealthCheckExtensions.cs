using System.Text.Json;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mealthy.Api.Shared.Infraestructure.Web;

public static class HealthCheckExtensions
{
    /// <summary>
    /// Health checks nativos:
    ///  - "api": siempre Healthy si el proceso responde.
    ///  - "postgres": AddDbContextCheck ejecuta CanConnectAsync() del AppDbContext (abre conexión con la misma
    ///    connection string que usa la app). Si Postgres no responde → Unhealthy.
    /// </summary>
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("api", () => HealthCheckResult.Healthy("La API responde."))
            .AddDbContextCheck<AppDbContext>("postgres");

        return services;
    }

    /// <summary>GET /health → 200 si todo está Healthy, 503 si algo falla. Visible en Scalar.</summary>
    public static RouteHandlerBuilder MapApiHealthChecks(this IEndpointRouteBuilder app)
        => app.MapGet("/health", async (HealthCheckService health, CancellationToken ct) =>
            {
                var report = await health.CheckHealthAsync(ct);

                var body = new
                {
                    status = report.Status.ToString(),
                    totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
                    checks = report.Entries.Select(e => new
                    {
                        name = e.Key,
                        status = e.Value.Status.ToString(),
                        durationMs = Math.Round(e.Value.Duration.TotalMilliseconds, 1),
                        description = e.Value.Description
                                      ?? (e.Value.Status == HealthStatus.Healthy ? null : "No se pudo conectar.")
                    })
                };

                return report.Status == HealthStatus.Healthy
                    ? Results.Ok(body)
                    : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
            })
            .WithName("Health")
            .WithTags("Health")
            .WithSummary("Estado de la API y de PostgreSQL")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            .AllowAnonymous()
            .DisableRateLimiting();
}
