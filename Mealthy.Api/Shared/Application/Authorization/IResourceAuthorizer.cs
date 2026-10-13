using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Shared.Application.Authorization;

/// <summary>
/// Puerto de autorización por recurso para la capa de aplicación. Los servicios ya no comparan
/// ids a mano ni reciben el userId para eso: preguntan "¿el usuario actual puede hacer X con esto?".
/// </summary>
public interface IResourceAuthorizer
{
    /// <summary>Lanza ForbiddenException (→ 403) si el usuario actual no puede hacer <paramref name="action"/>.</summary>
    Task EnsureAsync(IOwnedResource resource, ResourceAction action, CancellationToken ct = default);
}
