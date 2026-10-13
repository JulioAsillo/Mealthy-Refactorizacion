using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Marketplace.Domain.Model;

/// <summary>Tienda (antes "Market"). Modelo A: un StoreOwner posee exactamente una tienda.</summary>
public class Store : Entity, IOwnedResource
{
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Ruc { get; private set; } = null!;
    /// <summary>FK a images.id (contexto Media): logo o foto de la tienda.</summary>
    public int? ImageId { get; private set; }

    /// <summary>FK 1:1 a users.id. Sin navegación: Marketplace no depende del modelo de Security.</summary>
    public int OwnerId { get; private set; }

    // Requerido por EF Core
    private Store() { }

    public Store(int ownerId, string name, string description, string ruc, int? imageId)
    {
        OwnerId = ownerId;
        Ruc = ruc.Trim();
        Update(name, description, imageId);
    }

    /// <summary>El RUC no se edita: identifica legalmente a la tienda.</summary>
    public void Update(string name, string description, int? imageId)
    {
        Name = name.Trim();
        Description = description.Trim();
        ImageId = imageId;
    }

    public bool IsOwnedBy(int userId) => OwnerId == userId;
}
