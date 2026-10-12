using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Marketplace.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Marketplace.Infraestructure.Persistence;

public class ProductRepository(AppDbContext context) : BaseRepository<Product>(context), IProductRepository
{
    public Task<Product?> FindInStoreAsync(int storeId, int id, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(p => p.Id == id && p.StoreId == storeId, ct);

    public Task<Product?> GetByIdReadOnlyAsync(int id, CancellationToken ct = default)
        => Set.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(
        int? storeId, string? category, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking();

        if (storeId is { } sid)
            query = query.Where(p => p.StoreId == sid);

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category.Trim();
            query = query.Where(p => EF.Functions.ILike(p.Category, cat));   // igualdad sin mayúsculas
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
