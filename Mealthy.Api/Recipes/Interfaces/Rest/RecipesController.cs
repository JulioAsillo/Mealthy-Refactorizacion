using Mealthy.Api.Recipes.Application.Services;
using Mealthy.Api.Recipes.Interfaces.Rest.Mappings;
using Mealthy.Api.Recipes.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Infraestructure.Web;
using Mealthy.Api.Shared.Interfaces.Extensions;
using Mealthy.Api.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Mealthy.Api.Recipes.Interfaces.Rest;

[ApiController]
[Route("api/v1/recipes")]
[Authorize]
[EvictCacheTags(CacheTags.Recipes)]
[Produces("application/json")]
public class RecipesController(RecipeService recipes) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = CachePolicies.Recipes)]
    [ProducesResponseType<PagedResult<RecipeSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RecipeSummaryResponse>>> List(
        [FromQuery] PageQuery query, [FromQuery] string? search, CancellationToken ct)
    {
        var page = await recipes.ListAsync(query.SafePage, query.SafePageSize, search, ct);
        return Ok(new PagedResult<RecipeSummaryResponse>(
            page.Items.Select(r => r.ToSummaryResponse()).ToList(), page.Page, page.PageSize, page.TotalItems));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = CachePolicies.Recipes)]
    [ProducesResponseType<RecipeDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeDetailResponse>> GetById(int id, CancellationToken ct)
        => Ok((await recipes.GetAsync(id, ct)).ToDetailResponse());

    [HttpPost]
    [ProducesResponseType<RecipeDetailResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecipeDetailResponse>> Create(CreateRecipeRequest req, CancellationToken ct)
    {
        var recipe = await recipes.CreateAsync(User.GetUserId(), req.ToCommand(), ct);
        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe.ToDetailResponse());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<RecipeDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeDetailResponse>> Update(int id, UpdateRecipeRequest req, CancellationToken ct)
        => Ok((await recipes.UpdateAsync(id, User.GetUserId(), req.ToCommand(), ct)).ToDetailResponse());

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await recipes.DeleteAsync(id, User.GetUserId(), ct);
        return NoContent();
    }
}
