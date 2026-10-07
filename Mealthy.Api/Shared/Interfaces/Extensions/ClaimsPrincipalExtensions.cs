using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Mealthy.Api.Shared.Interfaces.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id del usuario autenticado (claim "sub"). Nunca se toma del body.</summary>
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return int.TryParse(sub, out var id)
            ? id
            : throw new InvalidOperationException("Token sin claim 'sub' válido.");
    }
}
