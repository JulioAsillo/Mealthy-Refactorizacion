using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Marketplace.Domain.Model;

/// <summary>Tienda (antes "Market"). Modelo A: un StoreOwner posee exactamente una tienda.</summary>
public class Store : Entity
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Ruc { get; private set; } = null!;
    public string? PhotoUrl { get; private set; }

    /// <summary>FK 1:1 a users.id. Sin navegación: Marketplace no depende del modelo de Security.</summary>
    public int OwnerId { get; private set; }

    // Requerido por EF Core
    private Store() { }

    public Store(int ownerId, string name, string description, string ruc, string? photoUrl)
    {
        OwnerId = ownerId;
        Ruc = ruc.Trim();
        Update(name, description, photoUrl);
    }

    /// <summary>El RUC no se edita: identifica legalmente a la tienda.</summary>
    public void Update(string name, string description, string? photoUrl)
    {
        Name = name.Trim();
        Description = description.Trim();
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
    }

    public bool IsOwnedBy(int userId) => OwnerId == userId;
}
