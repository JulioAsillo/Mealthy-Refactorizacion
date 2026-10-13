namespace Mealthy.Api.Shared.Infraestructure.Web;

public static class SecurityHeadersExtensions
{
    /// <summary>
    /// Cabeceras defensivas en todas las respuestas de la API. La API solo devuelve JSON y redirects,
    /// así que la CSP es la más estricta posible ("no cargues nada"). Scalar y el documento OpenAPI
    /// (solo en Development) quedan fuera porque son una página HTML con scripts.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.XContentTypeOptions = "nosniff";             // no adivinar el tipo de contenido
                headers.XFrameOptions = "DENY";                      // no se puede embeber en un iframe
                headers["Referrer-Policy"] = "no-referrer";
                headers["Cross-Origin-Opener-Policy"] = "same-origin";

                var path = context.Request.Path;
                if (!path.StartsWithSegments("/scalar") && !path.StartsWithSegments("/openapi"))
                    headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";

                return Task.CompletedTask;
            });

            await next();
        });
}
