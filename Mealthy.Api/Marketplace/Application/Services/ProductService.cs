using Mealthy.Api.Marketplace.Application.Commands;
using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Marketplace.Domain.Repositories;
using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Application.Authorization;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Marketplace.Application.Services;

public class ProductService(
    IProductRepository products,
    IStoreRepository stores,
    IImageAttachmentValidator images,
    IResourceAuthorizer authorizer,
    IUnitOfWorks unitOfWork)
{
    // ---------- Lecturas (públicas) ----------

    public async Task<PagedResult<Product>> SearchAsync(string? category, string? search, int page, int pageSize,
        CancellationToken ct = default)
    {
        var (items, total) = await products.SearchAsync(null, category, search, page, pageSize, ct);
        return new PagedResult<Product>(items, page, pageSize, total);
    }

    public async Task<PagedResult<Product>> ListByStoreAsync(int storeId, string? category, string? search,
        int page, int pageSize, CancellationToken ct = default)
    {
        if (!await stores.ExistsAsync(storeId, ct))
            throw new NotFoundException($"Tienda {storeId} no encontrada.");

        var (items, total) = await products.SearchAsync(storeId, category, search, page, pageSize, ct);
        return new PagedResult<Product>(items, page, pageSize, total);
    }

    public async Task<Product> GetAsync(int id, CancellationToken ct = default)
        => await products.GetByIdReadOnlyAsync(id, ct)
           ?? throw new NotFoundException($"Producto {id} no encontrado.");

    // ---------- Escrituras (dueño de la tienda; eliminar también un Admin) ----------

    public async Task<Product> CreateAsync(int storeId, ProductCommand cmd, CancellationToken ct = default)
    {
        var store = await LoadAuthorizedStoreAsync(storeId, ResourceAction.Manage, ct);
        await images.EnsureAttachableAsync(cmd.ImageId, store.OwnerId, ct);

        var product = new Product(storeId, cmd.Name, cmd.Category, cmd.Price, cmd.Unit, cmd.Stock, cmd.ImageId);
        await products.AddAsync(product, ct);
        await unitOfWork.CompleteAsync(ct);
        return product;
    }

    public async Task<Product> UpdateAsync(int storeId, int id, ProductCommand cmd, uint? expectedVersion,
        CancellationToken ct = default)
    {
        var store = await LoadAuthorizedStoreAsync(storeId, ResourceAction.Manage, ct);
        var product = await LoadInStoreAsync(storeId, id, ct);
        Concurrency.EnsureVersion(product, expectedVersion);
        if (cmd.ImageId != product.ImageId)   // solo se valida la foto nueva
            await images.EnsureAttachableAsync(cmd.ImageId, store.OwnerId, ct);

        product.Update(cmd.Name, cmd.Category, cmd.Price, cmd.Unit, cmd.Stock, cmd.ImageId);
        await unitOfWork.CompleteAsync(ct);
        return product;
    }

    public async Task DeleteAsync(int storeId, int id, CancellationToken ct = default)
    {
        await LoadAuthorizedStoreAsync(storeId, ResourceAction.Delete, ct);   // dueño o Admin
        var product = await LoadInStoreAsync(storeId, id, ct);

        products.Remove(product);   // borrado lógico (ISoftDeletable)
        await unitOfWork.CompleteAsync(ct);
    }

    // ---------- Privados ----------

    /// <summary>Un producto no tiene dueño propio: el permiso se decide sobre su tienda.</summary>
    private async Task<Store> LoadAuthorizedStoreAsync(int storeId, ResourceAction action, CancellationToken ct)
    {
        var store = await stores.FindByIdAsync(storeId, ct)
                    ?? throw new NotFoundException($"Tienda {storeId} no encontrada.");

        await authorizer.EnsureAsync(store, action, ct);
        return store;
    }

    private async Task<Product> LoadInStoreAsync(int storeId, int id, CancellationToken ct)
        => await products.FindInStoreAsync(storeId, id, ct)
           ?? throw new NotFoundException($"Producto {id} no encontrado en la tienda {storeId}.");
}
