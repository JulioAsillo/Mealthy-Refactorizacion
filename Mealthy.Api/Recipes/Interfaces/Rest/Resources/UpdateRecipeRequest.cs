using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Recipes.Interfaces.Rest.Resources;

/// <summary>Solo datos generales; ingredientes y pasos se editan en sus rutas anidadas.</summary>
public record UpdateRecipeRequest(
    [Required, StringLength(120, MinimumLength = 3)] string Title,
    [Required, StringLength(2000)] string Description,
    [Range(1, 1440)] int PreparationMinutes,
    [Range(1, 100)] int Servings,
    [Range(1, int.MaxValue)] int? ImageId);
