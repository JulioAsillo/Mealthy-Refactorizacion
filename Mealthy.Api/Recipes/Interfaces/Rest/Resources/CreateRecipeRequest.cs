namespace Mealthy.Api.Recipes.Interfaces.Rest.Resources;

/// <summary>Crea la receta y, opcionalmente, sus ingredientes y pasos en una sola llamada.</summary>
public record CreateRecipeRequest(
    string Title,
    string Description,
    int PreparationMinutes,
    int Servings,
    int? ImageId,
    IReadOnlyList<IngredientRequest>? Ingredients,
    IReadOnlyList<StepRequest>? Steps);
