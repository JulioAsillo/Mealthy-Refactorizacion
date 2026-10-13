namespace Mealthy.Api.Security.Interfaces.Rest;

/// <summary>
/// El refresh token viaja SOLO en una cookie:
///  - HttpOnly: JavaScript no puede leerla → un XSS no puede robar la sesión larga.
///  - SameSite=Strict: el navegador no la envía desde otros sitios → sin CSRF sobre /auth/refresh.
///  - Secure: solo HTTPS (los navegadores tratan http://localhost como seguro, así que funciona en dev).
///  - Path=/api/v1/auth: no viaja en el resto de peticiones a la API.
/// El access token (15 min) sigue yendo en el body y el frontend lo guarda solo en memoria.
/// </summary>
internal static class RefreshTokenCookie
{
    public const string Name = "mealthy_refresh";
    private const string Path = "/api/v1/auth";

    public static void Append(HttpResponse response, string token, DateTime expiresAtUtc)
        => response.Cookies.Append(Name, token, Options(DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc)));

    public static string? Read(HttpRequest request)
        => request.Cookies.TryGetValue(Name, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    public static void Delete(HttpResponse response) => response.Cookies.Delete(Name, Options(expiresAtUtc: null));

    private static CookieOptions Options(DateTime? expiresAtUtc) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = Path,
        IsEssential = true,
        Expires = expiresAtUtc is { } e ? new DateTimeOffset(e) : null
    };
}
