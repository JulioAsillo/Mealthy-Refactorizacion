namespace Mealthy.Api.Recipes.Interfaces.Rest.Resources;

public record IngredientRequest(
    string Name,
    decimal Quantity,
    string Unit);
