using Mealthy.Api.Recipes.Domain.Model;
using Mealthy.Api.Recipes.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

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
        IOrderedQueryable<Recipe> ordered;

        if (string.IsNullOrWhiteSpace(search))
        {
            ordered = query.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id);
        }
        else
        {
            // Full-text (título + descripción, con raíces en español) O coincidencia parcial en el título.
            // websearch_to_tsquery acepta la sintaxis de un buscador: "pollo -frito", comillas, OR.
            var term = search.Trim();
            var pattern = $"%{term}%";

            query = query.Where(r =>
                EF.Property<NpgsqlTsVector>(r, RecipeConfiguration.SearchVector)
                    .Matches(EF.Functions.WebSearchToTsQuery("spanish", term))
                || EF.Functions.ILike(r.Title, pattern));

            // Primero las más relevantes
            ordered = query
                .OrderByDescending(r => EF.Property<NpgsqlTsVector>(r, RecipeConfiguration.SearchVector)
                    .Rank(EF.Functions.WebSearchToTsQuery("spanish", term)))
                .ThenByDescending(r => r.CreatedAt)
                .ThenByDescending(r => r.Id);
        }

        var total = await query.CountAsync(ct);
        var items = await ordered
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
