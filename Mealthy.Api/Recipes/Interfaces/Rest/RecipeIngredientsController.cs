using Mealthy.Api.Recipes.Application.Services;
using Mealthy.Api.Recipes.Interfaces.Rest.Mappings;
using Mealthy.Api.Recipes.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Infraestructure.Web;
using Mealthy.Api.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Mealthy.Api.Recipes.Interfaces.Rest;

[ApiController]
[Route("api/v1/recipes/{recipeId:int}/ingredients")]
[Authorize]
[EvictCacheTags(CacheTags.Recipes)]   // cambiar ingredientes/pasos cambia el detalle de la receta
[Produces("application/json")]
public class RecipeIngredientsController(RecipeService recipes) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = CachePolicies.Recipes)]
    [ProducesResponseType<IReadOnlyList<IngredientResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<IngredientResponse>>> List(int recipeId, CancellationToken ct)
        => Ok((await recipes.GetIngredientsAsync(recipeId, ct)).Select(i => i.ToResponse()).ToList());

    [HttpPost]
    [ProducesResponseType<IngredientResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngredientResponse>> Add(int recipeId, IngredientRequest req, CancellationToken ct)
    {
        var ingredient = await recipes.AddIngredientAsync(recipeId, req.ToCommand(), ct);
        return CreatedAtAction(nameof(List), new { recipeId }, ingredient.ToResponse());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<IngredientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngredientResponse>> Update(int recipeId, int id, IngredientRequest req, CancellationToken ct)
        => Ok((await recipes.UpdateIngredientAsync(recipeId, id, req.ToCommand(), ct)).ToResponse());

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int recipeId, int id, CancellationToken ct)
    {
        await recipes.RemoveIngredientAsync(recipeId, id, ct);
        return NoContent();
    }
}
