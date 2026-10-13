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
[Route("api/v1/recipes/{recipeId:int}/steps")]
[Authorize]
[EvictCacheTags(CacheTags.Recipes)]   // cambiar ingredientes/pasos cambia el detalle de la receta
[Produces("application/json")]
public class RecipeStepsController(RecipeService recipes) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = CachePolicies.Recipes)]
    [ProducesResponseType<IReadOnlyList<StepResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StepResponse>>> List(int recipeId, CancellationToken ct)
        => Ok((await recipes.GetStepsAsync(recipeId, ct)).Select(s => s.ToResponse()).ToList());

    [HttpPost]
    [ProducesResponseType<StepResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StepResponse>> Add(int recipeId, StepRequest req, CancellationToken ct)
    {
        var step = await recipes.AddStepAsync(recipeId, req.ToCommand(), ct);
        return CreatedAtAction(nameof(List), new { recipeId }, step.ToResponse());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<StepResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StepResponse>> Update(int recipeId, int id, StepRequest req, CancellationToken ct)
        => Ok((await recipes.UpdateStepAsync(recipeId, id, req.ToCommand(), ct)).ToResponse());

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int recipeId, int id, CancellationToken ct)
    {
        await recipes.RemoveStepAsync(recipeId, id, ct);
        return NoContent();
    }
}
