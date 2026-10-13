using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mealthy.Api.Shared.Infraestructure.Web;

public static class HealthCheckExtensions
{
    /// <summary>
    /// GET /health → 200 si todo está Healthy, 503 si algo falla. Visible en Scalar y usado por Aspire
    /// (WithHttpHealthCheck) para saber cuándo la API está lista.
    ///
    /// Los checks ya no se registran aquí: cada integración aporta el suyo.
    ///  - "self": ServiceDefaults (el proceso responde).
    ///  - "AppDbContext": EnrichNpgsqlDbContext (abre conexión a PostgreSQL).
    ///  - Fases siguientes: Blob/Queue Storage, Redis… aparecen solos al agregar su integración.
    /// </summary>
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
            .WithSummary("Estado de la API y de sus dependencias")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status503ServiceUnavailable)
            .AllowAnonymous()
            .DisableRateLimiting();
}
