using Mealthy.Api.Pantry.Domain.Model;
using Mealthy.Api.Shared.Domain.Repositories;

namespace Mealthy.Api.Pantry.Domain.Repositories;

public interface ISupplyRepository : IBaseRepository<Supply>
{
    /// <summary>Insumo CON tracking, solo si pertenece al usuario. Null si no existe o es de otro.</summary>
    Task<Supply?> FindOwnedAsync(int id, int userId, CancellationToken ct = default);

    /// <summary>Despensa paginada del usuario, con búsqueda opcional por nombre.</summary>
    Task<(IReadOnlyList<Supply> Items, int Total)> ListPagedByUserAsync(
        int userId, int page, int pageSize, string? search, CancellationToken ct = default);
}
