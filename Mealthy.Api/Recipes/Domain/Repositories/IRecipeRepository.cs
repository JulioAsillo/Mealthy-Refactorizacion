using Mealthy.Api.Recipes.Domain.Model;
using Mealthy.Api.Shared.Domain.Repositories;

namespace Mealthy.Api.Recipes.Domain.Repositories;

public interface IRecipeRepository : IBaseRepository<Recipe>
{
    /// <summary>Receta con ingredientes y pasos, CON tracking. Para modificar el agregado.</summary>
    Task<Recipe?> FindAggregateAsync(int id, CancellationToken ct = default);

    /// <summary>Receta con ingredientes y pasos, SIN tracking. Para lecturas.</summary>
    Task<Recipe?> GetDetailsAsync(int id, CancellationToken ct = default);

    /// <summary>Listado paginado (sin hijos), con búsqueda opcional por título.</summary>
    Task<(IReadOnlyList<Recipe> Items, int Total)> ListPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct = default);
}
