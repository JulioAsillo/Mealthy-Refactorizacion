using Mealthy.Api.Recipes.Domain.Model;
using Mealthy.Api.Recipes.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Recipes.Infraestructure.Persistence;

public class RecipeRepository(AppDbContext context) : BaseRepository<Recipe>(context), IRecipeRepository
{
    public Task<Recipe?> FindAggregateAsync(int id, CancellationToken ct = default)
        => WithDetails(Set).FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Recipe?> GetDetailsAsync(int id, CancellationToken ct = default)
        => WithDetails(Set.AsNoTracking()).FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<(IReadOnlyList<Recipe> Items, int Total)> ListPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(r => EF.Functions.ILike(r.Title, pattern));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    private static IQueryable<Recipe> WithDetails(IQueryable<Recipe> query)
        => query
            .Include(r => r.Ingredients)
            .Include(r => r.Steps.OrderBy(s => s.Order))
            .AsSplitQuery();
}
