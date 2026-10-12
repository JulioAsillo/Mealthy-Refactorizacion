using Mealthy.Api.Security.Application.Services;
using Mealthy.Api.Security.Interfaces.Rest.Mappings;
using Mealthy.Api.Security.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Infraestructure.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Mealthy.Api.Security.Interfaces.Rest;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitingExtensions.AuthPolicy)]   // 10 req/min por IP en todo /auth
[Produces("application/json")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("sign-up")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> SignUp(SignUpRequest req, CancellationToken ct)
    {
        var result = await auth.SignUpAsync(req.ToCommand(), ct);
        return CreatedAtAction(nameof(UsersController.GetById), "Users",
            new { id = result.User.Id }, result.ToResponse());
    }

    [HttpPost("sign-in")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> SignIn(SignInRequest req, CancellationToken ct)
        => Ok((await auth.SignInAsync(req.ToCommand(), ct)).ToResponse());

    /// <summary>Canjea un refresh token por un access token nuevo + un refresh token nuevo (rotación).</summary>
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest req, CancellationToken ct)
        => Ok((await auth.RefreshAsync(req.ToCommand(), ct)).ToResponse());

    /// <summary>Revoca el refresh token (logout). El access token sigue vivo hasta que expire (máx. 15 min).</summary>
    [HttpPost("sign-out")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SignOutSession(RefreshTokenRequest req, CancellationToken ct)
    {
        await auth.SignOutAsync(req.ToCommand(), ct);
        return NoContent();
    }
}
