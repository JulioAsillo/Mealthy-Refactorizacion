using Mealthy.Api.Pantry.Application.Services;
using Mealthy.Api.Pantry.Interfaces.Rest.Mappings;
using Mealthy.Api.Pantry.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Interfaces.Extensions;
using Mealthy.Api.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mealthy.Api.Pantry.Interfaces.Rest;

/// <summary>/pantry = la despensa del usuario autenticado. No existe /pantry de otro usuario.</summary>
[ApiController]
[Route("api/v1/pantry")]
[Authorize]
[Produces("application/json")]
public class PantryController(PantryService pantry) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<SupplyResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SupplyResponse>>> List(
        [FromQuery] PageQuery query, [FromQuery] string? search, CancellationToken ct)
    {
        var page = await pantry.ListAsync(User.GetUserId(), query.SafePage, query.SafePageSize, search, ct);
        return Ok(new PagedResult<SupplyResponse>(
            page.Items.Select(s => s.ToResponse()).ToList(), page.Page, page.PageSize, page.TotalItems));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<SupplyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplyResponse>> GetById(int id, CancellationToken ct)
    {
        var supply = await pantry.GetAsync(id, User.GetUserId(), ct);
        Response.SetETag(supply);
        return Ok(supply.ToResponse());
    }

    [HttpPost]
    [Idempotent]
    [ProducesResponseType<SupplyResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SupplyResponse>> Create(SupplyRequest req, CancellationToken ct)
    {
        var supply = await pantry.CreateAsync(User.GetUserId(), req.ToCommand(), ct);
        return CreatedAtAction(nameof(GetById), new { id = supply.Id }, supply.ToResponse());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<SupplyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status412PreconditionFailed)]
    public async Task<ActionResult<SupplyResponse>> Update(int id, SupplyRequest req, CancellationToken ct)
    {
        var supply = await pantry.UpdateAsync(id, User.GetUserId(), req.ToCommand(), Request.GetIfMatchVersion(), ct);
        Response.SetETag(supply);
        return Ok(supply.ToResponse());
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await pantry.DeleteAsync(id, User.GetUserId(), ct);
        return NoContent();
    }
}
