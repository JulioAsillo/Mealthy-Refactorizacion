using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Mealthy.Api.Shared.Infraestructure.Web;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";

    /// <summary>
    /// Rate limiting nativo de ASP.NET Core. Política "auth": 10 peticiones por minuto por IP,
    /// para frenar fuerza bruta en sign-in / sign-up / refresh. Exceder el límite → 429.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            o.AddPolicy(AuthPolicy, http => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0                    // sin cola: se rechaza de inmediato
                }));

            // Respuesta 429 como ProblemDetails + header Retry-After
            o.OnRejected = async (ctx, ct) =>
            {
                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    ctx.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                var problems = ctx.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problems.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = ctx.HttpContext,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Demasiadas solicitudes",
                        Detail = "Has superado el límite de intentos. Espera un minuto y vuelve a intentarlo."
                    }
                });
            };
        });

        return services;
    }
}
