using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Shared.Domain.Repositories;

namespace Mealthy.Api.Marketplace.Domain.Repositories;

public interface IStoreRepository : IBaseRepository<Store>
{
    Task<Store?> FindByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<bool> ExistsByRucAsync(string ruc, CancellationToken ct = default);

    Task<(IReadOnlyList<Store> Items, int Total)> ListPagedAsync(
        int page, int pageSize, string? search, CancellationToken ct = default);
}
