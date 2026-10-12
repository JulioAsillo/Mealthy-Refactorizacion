using Mealthy.Api.Recipes.Application.Commands;
using Mealthy.Api.Recipes.Domain.Model;
using Mealthy.Api.Recipes.Domain.Repositories;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Recipes.Application.Services;

/// <summary>
/// Casos de uso del agregado Recipe. Ingredientes y pasos se modifican SIEMPRE a través de Recipe,
/// que es la raíz del agregado: no hay repositorios propios para Ingredient ni Step.
/// </summary>
public class RecipeService(IRecipeRepository recipes, IUnitOfWorks unitOfWork)
{
    // ---------- Lecturas (públicas) ----------

    public async Task<PagedResult<Recipe>> ListAsync(int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var (items, total) = await recipes.ListPagedAsync(page, pageSize, search, ct);
        return new PagedResult<Recipe>(items, page, pageSize, total);
    }

    public async Task<Recipe> GetAsync(int id, CancellationToken ct = default)
        => await recipes.GetDetailsAsync(id, ct)
           ?? throw new NotFoundException($"Receta {id} no encontrada.");

    // ---------- Receta ----------

    public async Task<Recipe> CreateAsync(int authorId, CreateRecipeCommand cmd, CancellationToken ct = default)
    {
        var recipe = new Recipe(authorId, cmd.Title, cmd.Description,
            cmd.PreparationMinutes, cmd.Servings, cmd.PhotoUrl);

        foreach (var i in cmd.Ingredients)
            recipe.AddIngredient(i.Name, i.Quantity, i.Unit);

        foreach (var s in cmd.Steps)
            recipe.AddStep(s.Description);

        await recipes.AddAsync(recipe, ct);
        await unitOfWork.CompleteAsync(ct);
        return recipe;
    }

    public async Task<Recipe> UpdateAsync(int id, int userId, UpdateRecipeCommand cmd, CancellationToken ct = default)
    {
        var recipe = await LoadOwnedAsync(id, userId, ct);
        recipe.Update(cmd.Title, cmd.Description, cmd.PreparationMinutes, cmd.Servings, cmd.PhotoUrl);
        await unitOfWork.CompleteAsync(ct);   // agregado trackeado: no hace falta Update()
        return recipe;
    }

    public async Task DeleteAsync(int id, int userId, CancellationToken ct = default)
    {
        var recipe = await LoadOwnedAsync(id, userId, ct);
        recipes.Remove(recipe);               // cascade borra ingredientes y pasos
        await unitOfWork.CompleteAsync(ct);
    }

    // ---------- Ingredientes ----------

    public async Task<IReadOnlyCollection<Ingredient>> GetIngredientsAsync(int recipeId, CancellationToken ct = default)
        => (await GetAsync(recipeId, ct)).Ingredients;

    public async Task<Ingredient> AddIngredientAsync(int recipeId, int userId, IngredientCommand cmd, CancellationToken ct = default)
    {
        var recipe = await LoadOwnedAsync(recipeId, userId, ct);
        var ingredient = recipe.AddIngredient(cmd.Name, cmd.Quantity, cmd.Unit);
        await unitOfWork.CompleteAsync(ct);   // aquí EF asigna ingredient.Id
        return ingredient;
    }

    public async Task<Ingredient> UpdateIngredientAsync(int recipeId, int ingredientId, int userId,
        IngredientCommand cmd, CancellationToken ct = default)
    {
        var recipe = await LoadOwnedAsync(recipeId, userId, ct);
        var ingredient = recipe.UpdateIngredient(ingredientId, cmd.Name, cmd.Quantity, cmd.Unit);
        await unitOfWork.CompleteAsync(ct);
        return ingredient;
    }

    public async Task RemoveIngredientAsync(int recipeId, int ingredientId, int userId, CancellationToken ct = default)
    {
        var recipe = await LoadOwnedAsync(recipeId, userId, ct);
        recipe.RemoveIngredient(ingredientId); // huérfano → EF lo borra (FK requerida)
        await unitOfWork.CompleteAsync(ct);
    }

    // ---------- Pasos ----------

    public async Task<IReadOnlyCollection<Step>> GetStepsAsync(int recipeId, CancellationToken ct = default)
        => (await GetAsync(recipeId, ct)).Steps;

    public async Task<Step> AddStepAsync(int recipeId, int userId, StepCommand cmd, CancellationToken ct = default)
    {
        var recipe = await LoadOwnedAsync(recipeId, userId, ct);
        var step = recipe.AddStep(cmd.Description);
        await unitOfWork.CompleteAsync(ct);
        return step;
    }

    public async Task<Step> UpdateStepAsync(int recipeId, int stepId, int userId,
        StepCommand cmd, CancellationToken ct = default)
    {
        var recipe = await LoadOwnedAsync(recipeId, userId, ct);
        var step = recipe.UpdateStep(stepId, cmd.Description);
        await unitOfWork.CompleteAsync(ct);
        return step;
    }

    public async Task RemoveStepAsync(int recipeId, int stepId, int userId, CancellationToken ct = default)
    {
        var recipe = await LoadOwnedAsync(recipeId, userId, ct);
        recipe.RemoveStep(stepId);             // también reordena los pasos restantes
        await unitOfWork.CompleteAsync(ct);
    }

    // ---------- Privados ----------

    /// <summary>Carga el agregado con tracking y verifica que el usuario sea el autor.</summary>
    private async Task<Recipe> LoadOwnedAsync(int id, int userId, CancellationToken ct)
    {
        var recipe = await recipes.FindAggregateAsync(id, ct)
                     ?? throw new NotFoundException($"Receta {id} no encontrada.");

        if (!recipe.IsOwnedBy(userId))
            throw new ForbiddenException("Solo el autor puede modificar esta receta.");

        return recipe;
    }
}
