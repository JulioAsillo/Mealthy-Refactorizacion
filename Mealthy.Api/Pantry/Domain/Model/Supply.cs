using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Pantry.Domain.Model;

/// <summary>Insumo de la despensa personal de un usuario.</summary>
public class Supply : Entity
{
    public string Name { get; private set; } = null!;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = null!;
    public DateOnly? ExpiresOn { get; private set; }

    /// <summary>FK a users.id. Se toma SIEMPRE del token, nunca del body.</summary>
    public int UserId { get; private set; }

    // Requerido por EF Core
    private Supply() { }

    public Supply(int userId, string name, decimal quantity, string unit, DateOnly? expiresOn)
    {
        UserId = userId;
        Update(name, quantity, unit, expiresOn);
    }

    public void Update(string name, decimal quantity, string unit, DateOnly? expiresOn)
    {
        if (quantity < 0)
            throw new BusinessRuleException("La cantidad no puede ser negativa.");

        Name = name.Trim();
        Quantity = quantity;
        Unit = unit.Trim();
        ExpiresOn = expiresOn;
    }
}
