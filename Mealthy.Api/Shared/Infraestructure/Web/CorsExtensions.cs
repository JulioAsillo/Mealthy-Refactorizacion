namespace Mealthy.Api.Shared.Infraestructure.Web;

public static class CorsExtensions
{
    public const string FrontendPolicy = "Frontend";

    /// <summary>
    /// Orígenes permitidos desde configuración ("Cors:AllowedOrigins"), nunca AllowAnyOrigin.
    /// Dev: appsettings.Development.json → http://localhost:5173 (Vite).
    /// </summary>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration config)
    {
        var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(o => o.AddPolicy(FrontendPolicy, p => p
            .WithOrigins(origins)
            .AllowAnyHeader()                         // Authorization, Content-Type...
            .AllowAnyMethod()
            .WithExposedHeaders("Location")           // para leer la URL de los 201 Created
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10))));

        return services;
    }
}
