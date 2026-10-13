using Mealthy.Api.Shared.Application.Authorization;
using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace Mealthy.Api.Security.Infraestructure.Auth;

/// <summary>Adaptador: IResourceAuthorizer (aplicación) → IAuthorizationService (ASP.NET Core).</summary>
public sealed class HttpContextResourceAuthorizer(
    IHttpContextAccessor httpContextAccessor,
    IAuthorizationService authorization) : IResourceAuthorizer
{
    public async Task EnsureAsync(IOwnedResource resource, ResourceAction action, CancellationToken ct = default)
    {
        var user = httpContextAccessor.HttpContext?.User
                   ?? throw new ForbiddenException("No hay un usuario autenticado en este contexto.");

        var result = await authorization.AuthorizeAsync(user, resource, new OwnedResourceRequirement(action));
        if (result.Succeeded) return;

        throw new ForbiddenException(action == ResourceAction.Delete
            ? "Solo el dueño o un administrador pueden eliminar este recurso."
            : "Solo el dueño puede modificar este recurso.");
    }
}
