using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Recipes.Interfaces.Rest.Resources;

public record StepRequest(
    [Required, StringLength(1000, MinimumLength = 1)] string Description);
