namespace Mealthy.Api.Recipes.Application.Commands;

public record IngredientCommand(string Name, decimal Quantity, string Unit);

public record StepCommand(string Description);

public record CreateRecipeCommand(
    string Title,
    string Description,
    int PreparationMinutes,
    int Servings,
    int? ImageId,
    IReadOnlyList<IngredientCommand> Ingredients,
    IReadOnlyList<StepCommand> Steps);

public record UpdateRecipeCommand(
    string Title,
    string Description,
    int PreparationMinutes,
    int Servings,
    int? ImageId);
