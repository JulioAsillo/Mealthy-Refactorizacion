using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Shared.Application.Authorization;
using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Interfaces.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Mealthy.Api.Security.Infraestructure.Auth;

/// <summary>
/// Autorización basada en recursos (nativa de ASP.NET Core). ÚNICO lugar con la regla:
///  - Manage → solo el dueño.
///  - Delete → el dueño o un Admin (moderación).
/// </summary>
public sealed class OwnedResourceAuthorizationHandler
    : AuthorizationHandler<OwnedResourceRequirement, IOwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, OwnedResourceRequirement requirement, IOwnedResource resource)
    {
        var user = context.User;

        if (user.FindFirst(JwtRegisteredClaimNames.Sub) is not null && resource.IsOwnedBy(user.GetUserId()))
            context.Succeed(requirement);
        else if (requirement.Action == ResourceAction.Delete && user.IsInRole(nameof(Role.Admin)))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
