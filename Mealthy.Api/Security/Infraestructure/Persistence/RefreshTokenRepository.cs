using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Security.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Security.Infraestructure.Persistence;

public class RefreshTokenRepository(AppDbContext context)
    : BaseRepository<RefreshToken>(context), IRefreshTokenRepository
{
    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyList<RefreshToken>> ListActiveByUserAsync(int userId, DateTime now,
        CancellationToken ct = default)
        => await Set.Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
                    .ToListAsync(ct);
}
