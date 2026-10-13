using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Mealthy.Api.Shared.Infraestructure.Web;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";
    public const string UploadPolicy = "uploads";

    /// <summary>
    /// Rate limiting nativo de ASP.NET Core. Exceder el límite → 429 + Retry-After.
    ///  - "auth": 10 peticiones/min por IP, para frenar fuerza bruta en sign-in / sign-up / refresh.
    ///  - "uploads": 30 URLs de subida/min por usuario (por IP si no hay token), para que nadie
    ///    llene el storage.
    ///  - Global: toda escritura (POST/PUT/PATCH/DELETE) comparte un token bucket de 60/min por usuario.
    ///    Las lecturas no se limitan aquí (las protege el output cache).
    /// Requiere UseRateLimiter() DESPUÉS de UseAuthentication() para conocer al usuario.
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

            o.AddPolicy(UploadPolicy, http => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: UserOrIp(http),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

            o.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
                IsWrite(http.Request.Method)
                    ? RateLimitPartition.GetTokenBucketLimiter(UserOrIp(http), _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 60,                       // ráfaga máxima
                        TokensPerPeriod = 60,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    })
                    : RateLimitPartition.GetNoLimiter("reads"));

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

    private static string UserOrIp(HttpContext http)
        => http.User.FindFirstValue(JwtRegisteredClaimNames.Sub) is { } userId
            ? $"user:{userId}"
            : $"ip:{http.Connection.RemoteIpAddress}";

    private static bool IsWrite(string method)
        => HttpMethods.IsPost(method) || HttpMethods.IsPut(method)
           || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);
}
