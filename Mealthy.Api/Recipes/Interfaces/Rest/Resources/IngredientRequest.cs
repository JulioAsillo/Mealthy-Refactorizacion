using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Recipes.Interfaces.Rest.Resources;

public record IngredientRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Range(0.01, 99999999.99)] decimal Quantity,
    [Required, StringLength(20, MinimumLength = 1)] string Unit);
