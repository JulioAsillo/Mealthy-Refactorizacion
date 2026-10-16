using Mealthy.Api.Recipes.Application.Commands;
using Mealthy.Api.Recipes.Domain.Model;
using Mealthy.Api.Recipes.Interfaces.Rest.Resources;

namespace Mealthy.Api.Recipes.Interfaces.Rest.Mappings;

/// <summary>Mapeo manual Resource ↔ Command / Entity ↔ Resource (sin AutoMapper).</summary>
public static class RecipeMappings
{
    // ---------- Request → Command ----------

    public static CreateRecipeCommand ToCommand(this CreateRecipeRequest r) =>
        new(r.Title, r.Description, r.PreparationMinutes, r.Servings, r.ImageId,
            (r.Ingredients ?? []).Select(i => i.ToCommand()).ToList(),
            (r.Steps ?? []).Select(s => s.ToCommand()).ToList());

    public static UpdateRecipeCommand ToCommand(this UpdateRecipeRequest r) =>
        new(r.Title, r.Description, r.PreparationMinutes, r.Servings, r.ImageId);

    public static IngredientCommand ToCommand(this IngredientRequest r) => new(r.Name, r.Quantity, r.Unit);

    public static StepCommand ToCommand(this StepRequest r) => new(r.Description);

    // ---------- Entity → Response ----------

    public static IngredientResponse ToResponse(this Ingredient i) => new(i.Id, i.Name, i.Quantity, i.Unit);

    public static StepResponse ToResponse(this Step s) => new(s.Id, s.Order, s.Description);

    public static RecipeSummaryResponse ToSummaryResponse(this Recipe r) =>
        new(r.Id, r.Title, r.Description, r.PreparationMinutes, r.Servings, r.ImageId, r.AuthorId, r.CreatedAt);

    public static RecipeDetailResponse ToDetailResponse(this Recipe r) =>
        new(r.Id, r.Title, r.Description, r.PreparationMinutes, r.Servings, r.ImageId, r.AuthorId,
            r.CreatedAt, r.UpdatedAt, r.Version,
            r.Ingredients.Select(i => i.ToResponse()).ToList(),
            r.Steps.OrderBy(s => s.Order).Select(s => s.ToResponse()).ToList());
}
