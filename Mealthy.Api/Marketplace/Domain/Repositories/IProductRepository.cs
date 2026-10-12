using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Shared.Domain.Repositories;

namespace Mealthy.Api.Marketplace.Domain.Repositories;

public interface IProductRepository : IBaseRepository<Product>
{
    /// <summary>Producto CON tracking, solo si pertenece a esa tienda.</summary>
    Task<Product?> FindInStoreAsync(int storeId, int id, CancellationToken ct = default);

    Task<Product?> GetByIdReadOnlyAsync(int id, CancellationToken ct = default);

    /// <summary>Búsqueda paginada. storeId/category/search son filtros opcionales y combinables.</summary>
    Task<(IReadOnlyList<Product> Items, int Total)> SearchAsync(
        int? storeId, string? category, string? search, int page, int pageSize, CancellationToken ct = default);
}
