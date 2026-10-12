using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Security.Infraestructure.Auth;

public class JwtOptions
{
    public const string Section = "Jwt";

    [Required] public string Issuer { get; init; } = null!;
    [Required] public string Audience { get; init; } = null!;

    /// <summary>Secreto HMAC. NUNCA en appsettings: User Secrets en dev, variable de entorno / Key Vault en prod.</summary>
    [Required, MinLength(32)] public string Key { get; init; } = null!;

    /// <summary>Vida del access token. Con refresh tokens conviene que sea corta (15 min).</summary>
    [Range(1, 1440)] public int ExpirationMinutes { get; init; } = 15;

    /// <summary>Vida del refresh token.</summary>
    [Range(1, 90)] public int RefreshTokenDays { get; init; } = 7;
}
