namespace Mealthy.Api.Shared.Domain.Model;

/// <summary>
/// Recurso con dueño (receta → autor, tienda → dueño, imagen → quien la subió).
/// Las reglas de quién puede hacer qué viven en un solo lugar: OwnedResourceAuthorizationHandler.
/// </summary>
public interface IOwnedResource
{
    bool IsOwnedBy(int userId);
}
