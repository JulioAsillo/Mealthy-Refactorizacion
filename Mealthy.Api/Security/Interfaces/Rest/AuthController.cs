using Mealthy.Api.Security.Application.Commands;
using Mealthy.Api.Security.Application.Services;
using Mealthy.Api.Security.Interfaces.Rest.Mappings;
using Mealthy.Api.Security.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;
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
        RefreshTokenCookie.Append(Response, result.RefreshToken, result.RefreshTokenExpiresAt);
        return CreatedAtAction(nameof(UsersController.GetById), "Users",
            new { id = result.User.Id }, result.ToResponse());
    }

    [HttpPost("sign-in")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> SignIn(SignInRequest req, CancellationToken ct)
    {
        var result = await auth.SignInAsync(req.ToCommand(), ct);
        RefreshTokenCookie.Append(Response, result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Lee el refresh token de la cookie HttpOnly y lo canjea por un access token nuevo.
    /// Rotación: la cookie se reemplaza por un refresh token nuevo en cada llamada.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken ct)
    {
        var token = RefreshTokenCookie.Read(Request)
                    ?? throw new AuthenticationFailedException("No hay una sesión que renovar.");

        var result = await auth.RefreshAsync(new RefreshTokenCommand(token), ct);
        RefreshTokenCookie.Append(Response, result.RefreshToken, result.RefreshTokenExpiresAt);
        return Ok(result.ToResponse());
    }

    /// <summary>Revoca el refresh token y borra la cookie. El access token vive hasta expirar (máx. 15 min).</summary>
    [HttpPost("sign-out")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SignOutSession(CancellationToken ct)
    {
        if (RefreshTokenCookie.Read(Request) is { } token)
            await auth.SignOutAsync(new RefreshTokenCommand(token), ct);

        RefreshTokenCookie.Delete(Response);
        return NoContent();
    }
}
