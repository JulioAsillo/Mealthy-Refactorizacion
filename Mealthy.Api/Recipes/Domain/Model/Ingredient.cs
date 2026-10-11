
namespace Mealthy.Api.Recipes.Domain.Model;
using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

public class Ingredient : Entity
{
    public string Name { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = null!;
    public int RecipeId { get; private set; }

    private Ingredient() { }

    // internal: solo Recipe puede crear ingredientes
    internal Ingredient(string name, decimal quantity, string unit) => Update(name, quantity, unit);

    internal void Update(string name, decimal quantity, string unit)
    {
        if (quantity <= 0)
            throw new BusinessRuleException("La cantidad debe ser mayor a 0.");

        Name = name.Trim();
        Quantity = quantity;
        Unit = unit.Trim();
    }
}