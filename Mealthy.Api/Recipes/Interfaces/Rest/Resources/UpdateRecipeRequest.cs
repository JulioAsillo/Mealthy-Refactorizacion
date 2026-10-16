namespace Mealthy.Api.Recipes.Interfaces.Rest.Resources;

/// <summary>Solo datos generales; ingredientes y pasos se editan en sus rutas anidadas.</summary>
public record UpdateRecipeRequest(
    string Title,
    string Description,
    int PreparationMinutes,
    int Servings,
    int? ImageId);
