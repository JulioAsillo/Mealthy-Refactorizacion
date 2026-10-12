using Mealthy.Api.Marketplace.Application.Services;
using Mealthy.Api.Marketplace.Interfaces.Rest.Mappings;
using Mealthy.Api.Marketplace.Interfaces.Rest.Resources;
using Mealthy.Api.Security.Infraestructure.Auth;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Interfaces.Extensions;
using Mealthy.Api.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mealthy.Api.Marketplace.Interfaces.Rest;

[ApiController]
[Route("api/v1/stores")]
[Authorize(Policy = AuthPolicies.StoreOwnerOnly)]
[Produces("application/json")]
public class StoresController(StoreService stores) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<PagedResult<StoreResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<StoreResponse>>> List(
        [FromQuery] PageQuery query, [FromQuery] string? search, CancellationToken ct)
    {
        var page = await stores.ListAsync(query.SafePage, query.SafePageSize, search, ct);
        return Ok(new PagedResult<StoreResponse>(
            page.Items.Select(s => s.ToResponse()).ToList(), page.Page, page.PageSize, page.TotalItems));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType<StoreResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreResponse>> GetById(int id, CancellationToken ct)
        => Ok((await stores.GetAsync(id, ct)).ToResponse());

    /// <summary>La tienda del StoreOwner autenticado (útil para el frontend tras el login).</summary>
    [HttpGet("me")]
    [ProducesResponseType<StoreResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreResponse>> GetMine(CancellationToken ct)
        => Ok((await stores.GetByOwnerAsync(User.GetUserId(), ct)).ToResponse());

    [HttpPost]
    [ProducesResponseType<StoreResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StoreResponse>> Create(CreateStoreRequest req, CancellationToken ct)
    {
        var store = await stores.CreateAsync(User.GetUserId(), req.ToCommand(), ct);
        return CreatedAtAction(nameof(GetById), new { id = store.Id }, store.ToResponse());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<StoreResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreResponse>> Update(int id, UpdateStoreRequest req, CancellationToken ct)
        => Ok((await stores.UpdateAsync(id, User.GetUserId(), req.ToCommand(), ct)).ToResponse());
}
