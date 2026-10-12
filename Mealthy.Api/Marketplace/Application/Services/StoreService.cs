using Mealthy.Api.Marketplace.Application.Commands;
using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Marketplace.Domain.Repositories;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Marketplace.Application.Services;

public class StoreService(IStoreRepository stores, IUnitOfWorks unitOfWork)
{
    public async Task<PagedResult<Store>> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var (items, total) = await stores.ListPagedAsync(page, pageSize, search, ct);
        return new PagedResult<Store>(items, page, pageSize, total);
    }

    public async Task<Store> GetAsync(int id, CancellationToken ct = default)
        => await stores.FindByIdAsync(id, ct)
           ?? throw new NotFoundException($"Tienda {id} no encontrada.");

    public async Task<Store> GetByOwnerAsync(int ownerId, CancellationToken ct = default)
        => await stores.FindByOwnerAsync(ownerId, ct)
           ?? throw new NotFoundException("Aún no has registrado tu tienda.");

    public async Task<Store> CreateAsync(int ownerId, CreateStoreCommand cmd, CancellationToken ct = default)
    {
        if (await stores.ExistsByOwnerAsync(ownerId, ct))
            throw new ConflictException("Ya tienes una tienda registrada.");
        if (await stores.ExistsByRucAsync(cmd.Ruc, ct))
            throw new ConflictException("El RUC ya está registrado por otra tienda.");

        var store = new Store(ownerId, cmd.Name, cmd.Description, cmd.Ruc, cmd.PhotoUrl);
        await stores.AddAsync(store, ct);
        await unitOfWork.CompleteAsync(ct);
        return store;
    }

    public async Task<Store> UpdateAsync(int id, int userId, UpdateStoreCommand cmd, CancellationToken ct = default)
    {
        var store = await GetAsync(id, ct);   // FindByIdAsync del base trackea
        if (!store.IsOwnedBy(userId))
            throw new ForbiddenException("Solo el dueño puede modificar esta tienda.");

        store.Update(cmd.Name, cmd.Description, cmd.PhotoUrl);
        await unitOfWork.CompleteAsync(ct);
        return store;
    }
}
