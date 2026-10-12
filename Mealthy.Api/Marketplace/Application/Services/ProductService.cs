using Mealthy.Api.Marketplace.Application.Commands;
using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Marketplace.Domain.Repositories;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Marketplace.Application.Services;

public class ProductService(IProductRepository products, IStoreRepository stores, IUnitOfWorks unitOfWork)
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

    // ---------- Escrituras (solo el dueño de la tienda) ----------

    public async Task<Product> CreateAsync(int storeId, int userId, ProductCommand cmd, CancellationToken ct = default)
    {
        await EnsureStoreOwnerAsync(storeId, userId, ct);

        var product = new Product(storeId, cmd.Name, cmd.Category, cmd.Price, cmd.Unit, cmd.Stock, cmd.PhotoUrl);
        await products.AddAsync(product, ct);
        await unitOfWork.CompleteAsync(ct);
        return product;
    }

    public async Task<Product> UpdateAsync(int storeId, int id, int userId, ProductCommand cmd, CancellationToken ct = default)
    {
        await EnsureStoreOwnerAsync(storeId, userId, ct);
        var product = await LoadInStoreAsync(storeId, id, ct);

        product.Update(cmd.Name, cmd.Category, cmd.Price, cmd.Unit, cmd.Stock, cmd.PhotoUrl);
        await unitOfWork.CompleteAsync(ct);
        return product;
    }

    public async Task DeleteAsync(int storeId, int id, int userId, CancellationToken ct = default)
    {
        await EnsureStoreOwnerAsync(storeId, userId, ct);
        var product = await LoadInStoreAsync(storeId, id, ct);

        products.Remove(product);
        await unitOfWork.CompleteAsync(ct);
    }

    // ---------- Privados ----------

    private async Task EnsureStoreOwnerAsync(int storeId, int userId, CancellationToken ct)
    {
        var store = await stores.FindByIdAsync(storeId, ct)
                    ?? throw new NotFoundException($"Tienda {storeId} no encontrada.");

        if (!store.IsOwnedBy(userId))
            throw new ForbiddenException("Solo el dueño de la tienda puede gestionar sus productos.");
    }

    private async Task<Product> LoadInStoreAsync(int storeId, int id, CancellationToken ct)
        => await products.FindInStoreAsync(storeId, id, ct)
           ?? throw new NotFoundException($"Producto {id} no encontrado en la tienda {storeId}.");
}
