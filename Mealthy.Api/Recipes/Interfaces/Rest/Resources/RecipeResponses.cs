namespace Mealthy.Api.Recipes.Interfaces.Rest.Resources;

public record IngredientResponse(int Id, string Name, decimal Quantity, string Unit);

public record StepResponse(int Id, int Order, string Description);

/// <summary>Versión liviana para listados (sin hijos).</summary>
public record RecipeSummaryResponse(
    int Id,
    string Title,
    string Description,
    int PreparationMinutes,
    int Servings,
    string? PhotoUrl,
    int AuthorId,
    DateTime CreatedAt);

/// <summary>Receta completa para GET /recipes/{id}.</summary>
public record RecipeDetailResponse(
    int Id,
    string Title,
    string Description,
    int PreparationMinutes,
    int Servings,
    string? PhotoUrl,
    int AuthorId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<IngredientResponse> Ingredients,
    IReadOnlyList<StepResponse> Steps);
