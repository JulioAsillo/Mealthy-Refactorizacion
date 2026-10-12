using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Security.Domain.Model;

/// <summary>
/// Refresh token opaco con rotación. En BD solo se guarda el HASH (SHA-256), nunca el token:
/// si la tabla se filtra, los tokens no sirven.
/// </summary>
public class RefreshToken : Entity
{
    public int UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    /// <summary>Hash del token que lo reemplazó al rotar. Permite detectar reutilización.</summary>
    public string? ReplacedByTokenHash { get; private set; }

    // Requerido por EF Core
    private RefreshToken() { }

    public RefreshToken(int userId, string tokenHash, DateTime expiresAt)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public bool IsActive(DateTime now) => RevokedAt is null && ExpiresAt > now;

    public void Revoke(DateTime now, string? replacedByTokenHash = null)
    {
        RevokedAt ??= now;
        ReplacedByTokenHash ??= replacedByTokenHash;
    }
}
