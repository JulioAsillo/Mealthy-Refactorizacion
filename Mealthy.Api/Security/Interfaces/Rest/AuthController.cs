using Mealthy.Api.Security.Application.Services;
using Mealthy.Api.Security.Interfaces.Rest.Mappings;
using Mealthy.Api.Security.Interfaces.Rest.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mealthy.Api.Security.Interfaces.Rest;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
[Produces("application/json")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("sign-up")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> SignUp(SignUpRequest req, CancellationToken ct)
    {
        var result = await auth.SignUpAsync(req.ToCommand(), ct);
        return CreatedAtAction(nameof(UsersController.GetById), "Users",
            new { id = result.User.Id }, result.ToResponse());
    }

    [HttpPost("sign-in")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> SignIn(SignInRequest req, CancellationToken ct)
        => Ok((await auth.SignInAsync(req.ToCommand(), ct)).ToResponse());
}
