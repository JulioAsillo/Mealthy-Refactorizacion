using Mealthy.Api.Pantry.Domain.Model;
using Mealthy.Api.Pantry.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Pantry.Infraestructure.Persistence;

public class SupplyRepository(AppDbContext context) : BaseRepository<Supply>(context), ISupplyRepository
{
    public Task<Supply?> FindOwnedAsync(int id, int userId, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId, ct);

    public async Task<(IReadOnlyList<Supply> Items, int Total)> ListPagedByUserAsync(
        int userId, int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Where(s => s.UserId == userId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(s => EF.Functions.ILike(s.Name, pattern));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(s => s.Name)
            .ThenBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
