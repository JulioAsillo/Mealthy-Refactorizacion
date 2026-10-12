using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Shared.Domain.Repositories;

namespace Mealthy.Api.Security.Domain.Repositories;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    /// <summary>Token CON tracking por su hash.</summary>
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Tokens aún no revocados del usuario, CON tracking (para revocarlos en bloque).</summary>
    Task<IReadOnlyList<RefreshToken>> ListActiveByUserAsync(int userId, DateTime now, CancellationToken ct = default);
}
