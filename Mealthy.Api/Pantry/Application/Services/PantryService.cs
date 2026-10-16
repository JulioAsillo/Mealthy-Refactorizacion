using Mealthy.Api.Pantry.Application.Commands;
using Mealthy.Api.Pantry.Domain.Model;
using Mealthy.Api.Pantry.Domain.Repositories;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Pantry.Application.Services;

/// <summary>
/// Despensa personal. Todas las operaciones reciben el userId del token.
/// Un insumo ajeno responde 404 (no 403) para no revelar que existe.
/// </summary>
public class PantryService(ISupplyRepository supplies, IUnitOfWorks unitOfWork)
{
    public async Task<PagedResult<Supply>> ListAsync(int userId, int page, int pageSize, string? search,
        CancellationToken ct = default)
    {
        var (items, total) = await supplies.ListPagedByUserAsync(userId, page, pageSize, search, ct);
        return new PagedResult<Supply>(items, page, pageSize, total);
    }

    public Task<Supply> GetAsync(int id, int userId, CancellationToken ct = default)
        => LoadOwnedAsync(id, userId, ct);

    public async Task<Supply> CreateAsync(int userId, SupplyCommand cmd, CancellationToken ct = default)
    {
        var supply = new Supply(userId, cmd.Name, cmd.Quantity, cmd.Unit, cmd.ExpiresOn);
        await supplies.AddAsync(supply, ct);
        await unitOfWork.CompleteAsync(ct);
        return supply;
    }

    public async Task<Supply> UpdateAsync(int id, int userId, SupplyCommand cmd, uint? expectedVersion,
        CancellationToken ct = default)
    {
        var supply = await LoadOwnedAsync(id, userId, ct);
        Concurrency.EnsureVersion(supply, expectedVersion);
        supply.Update(cmd.Name, cmd.Quantity, cmd.Unit, cmd.ExpiresOn);
        await unitOfWork.CompleteAsync(ct);
        return supply;
    }

    public async Task DeleteAsync(int id, int userId, CancellationToken ct = default)
    {
        var supply = await LoadOwnedAsync(id, userId, ct);
        supplies.Remove(supply);
        await unitOfWork.CompleteAsync(ct);
    }

    private async Task<Supply> LoadOwnedAsync(int id, int userId, CancellationToken ct)
        => await supplies.FindOwnedAsync(id, userId, ct)
           ?? throw new NotFoundException($"Insumo {id} no encontrado en tu despensa.");
}
