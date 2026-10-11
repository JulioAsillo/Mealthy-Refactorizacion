using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Recipes.Domain.Model;

public class Step : Entity
{
    public string Description { get; private set; } = null!;
    public int Order { get; private set; }
    public int RecipeId { get; private set; }

    private Step() { }

    internal Step(string description, int order)
    {
        Update(description);
        Order = order;
    }

    internal void Update(string description) => Description = description.Trim();

    internal void SetOrder(int order) => Order = order;
}