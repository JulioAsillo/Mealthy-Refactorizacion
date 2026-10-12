using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Recipes.Interfaces.Rest.Resources;

/// <summary>Crea la receta y, opcionalmente, sus ingredientes y pasos en una sola llamada.</summary>
public record CreateRecipeRequest(
    [Required, StringLength(120, MinimumLength = 3)] string Title,
    [Required, StringLength(2000)] string Description,
    [Range(1, 1440)] int PreparationMinutes,
    [Range(1, 100)] int Servings,
    [Url, StringLength(500)] string? PhotoUrl,
    [MaxLength(50)] IReadOnlyList<IngredientRequest>? Ingredients,
    [MaxLength(50)] IReadOnlyList<StepRequest>? Steps);
