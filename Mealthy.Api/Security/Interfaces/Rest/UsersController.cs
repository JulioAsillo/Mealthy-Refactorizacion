using Mealthy.Api.Security.Application.Services;
using Mealthy.Api.Security.Infraestructure.Auth;
using Mealthy.Api.Security.Interfaces.Rest.Mappings;
using Mealthy.Api.Security.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Interfaces.Extensions;
using Mealthy.Api.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mealthy.Api.Security.Interfaces.Rest;

[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
public class UsersController(UserService users) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken ct)
        => Ok((await users.GetByIdAsync(User.GetUserId(), ct)).ToResponse());

    [HttpPut("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> UpdateMe(UpdateProfileRequest req, CancellationToken ct)
        => Ok((await users.UpdateProfileAsync(User.GetUserId(), req.ToCommand(), ct)).ToResponse());

    [HttpGet]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType<PagedResult<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserResponse>>> List([FromQuery] PageQuery query, CancellationToken ct)
    {
        var page = await users.ListAsync(query.SafePage, query.SafePageSize, ct);
        return Ok(new PagedResult<UserResponse>(
            page.Items.Select(u => u.ToResponse()).ToList(), page.Page, page.PageSize, page.TotalItems));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken ct)
        => Ok((await users.GetByIdAsync(id, ct)).ToResponse());

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await users.DeleteAsync(id, ct);
        return NoContent();
    }
}
