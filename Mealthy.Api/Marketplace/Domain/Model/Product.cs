using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Marketplace.Domain.Model;

/// <summary>
/// Producto publicado por una tienda. Es su propio agregado (no se carga dentro de Store),
/// porque una tienda puede tener cientos de productos.
/// </summary>
public class Product : Entity
{
    public string Name { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public decimal Price { get; private set; }
    public string Unit { get; private set; } = null!;
    public int Stock { get; private set; }
    public string? PhotoUrl { get; private set; }

    /// <summary>FK real a stores.id (antes era un entero suelto).</summary>
    public int StoreId { get; private set; }

    // Requerido por EF Core
    private Product() { }

    public Product(int storeId, string name, string category, decimal price, string unit, int stock, string? photoUrl)
    {
        StoreId = storeId;
        Update(name, category, price, unit, stock, photoUrl);
    }

    public void Update(string name, string category, decimal price, string unit, int stock, string? photoUrl)
    {
        if (price <= 0)
            throw new BusinessRuleException("El precio debe ser mayor a 0.");
        if (stock < 0)
            throw new BusinessRuleException("El stock no puede ser negativo.");

        Name = name.Trim();
        Category = category.Trim();
        Price = decimal.Round(price, 2);
        Unit = unit.Trim();
        Stock = stock;
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
    }
}
