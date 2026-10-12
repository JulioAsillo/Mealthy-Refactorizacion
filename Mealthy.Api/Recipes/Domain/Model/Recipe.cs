using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Recipes.Domain.Model;

public class Recipe : Entity
{
    private readonly List<Ingredient> _ingredients = [];
    private readonly List<Step> _steps = [];

    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;

    public int PreparationMinutes { get; private set; }
    public int Servings { get; private set; }
    public string? PhotoUrl { get; private set; }

    /// <summary>FK a users.id. Sin propiedad de navegación: Recipes no depende del modelo de Security.</summary>
    public int AuthorId { get; private set; }

    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients.AsReadOnly();
    public IReadOnlyCollection<Step> Steps => _steps.AsReadOnly();

    // Requerido por EF Core
    private Recipe() { }

    public Recipe(int authorId, string title, string description,
        int preparationMinutes, int servings, string? photoUrl)
    {
        AuthorId = authorId;
        SetDetails(title, description, preparationMinutes, servings, photoUrl);
    }

    public void Update(string title, string description, int preparationMinutes, int servings, string? photoUrl)
        => SetDetails(title, description, preparationMinutes, servings, photoUrl);

    public bool IsOwnedBy(int userId) => AuthorId == userId;

    // ---------- Ingredientes ----------
    public Ingredient AddIngredient(string name, decimal quantity, string unit)
    {
        var ingredient = new Ingredient(name, quantity, unit);
        _ingredients.Add(ingredient);
        return ingredient;
    }

    public Ingredient UpdateIngredient(int ingredientId, string name, decimal quantity, string unit)
    {
        var ingredient = FindIngredient(ingredientId);
        ingredient.Update(name, quantity, unit);
        return ingredient;
    }

    public void RemoveIngredient(int ingredientId) => _ingredients.Remove(FindIngredient(ingredientId));

    // ---------- Pasos ----------
    public Step AddStep(string description)
    {
        var nextOrder = _steps.Count == 0 ? 1 : _steps.Max(s => s.Order) + 1;
        var step = new Step(description, nextOrder);
        _steps.Add(step);
        return step;
    }

    public Step UpdateStep(int stepId, string description)
    {
        var step = FindStep(stepId);
        step.Update(description);
        return step;
    }

    public void RemoveStep(int stepId)
    {
        _steps.Remove(FindStep(stepId));
        Resequence();   // mantiene el orden contiguo: 1, 2, 3...
    }

    // ---------- Privados ----------
    private void SetDetails(string title, string description, int preparationMinutes, int servings, string? photoUrl)
    {
        if (preparationMinutes <= 0)
            throw new BusinessRuleException("El tiempo de preparación debe ser mayor a 0 minutos.");
        if (servings <= 0)
            throw new BusinessRuleException("Las porciones deben ser mayores a 0.");

        Title = title.Trim();
        Description = description.Trim();
        PreparationMinutes = preparationMinutes;
        Servings = servings;
        PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim();
    }

    private void Resequence()
    {
        var order = 1;
        foreach (var step in _steps.OrderBy(s => s.Order))
            step.SetOrder(order++);
    }

    private Ingredient FindIngredient(int id)
        => _ingredients.FirstOrDefault(i => i.Id == id)
           ?? throw new NotFoundException($"Ingrediente {id} no encontrado en esta receta.");

    private Step FindStep(int id)
        => _steps.FirstOrDefault(s => s.Id == id)
           ?? throw new NotFoundException($"Paso {id} no encontrado en esta receta.");


}