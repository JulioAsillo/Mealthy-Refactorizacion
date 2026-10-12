using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Marketplace.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Marketplace.Infraestructure.Persistence;

public class StoreRepository(AppDbContext context) : BaseRepository<Store>(context), IStoreRepository
{
    public Task<Store?> FindByOwnerAsync(int ownerId, CancellationToken ct = default)
        => Set.AsNoTracking().FirstOrDefaultAsync(s => s.OwnerId == ownerId, ct);

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => Set.AnyAsync(s => s.Id == id, ct);

    public Task<bool> ExistsByOwnerAsync(int ownerId, CancellationToken ct = default)
        => Set.AnyAsync(s => s.OwnerId == ownerId, ct);

    public Task<bool> ExistsByRucAsync(string ruc, CancellationToken ct = default)
        => Set.AnyAsync(s => s.Ruc == ruc.Trim(), ct);

    public async Task<(IReadOnlyList<Store> Items, int Total)> ListPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking();

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
