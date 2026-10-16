using Mealthy.Api.Pantry.Application.Commands;
using Mealthy.Api.Pantry.Domain.Model;
using Mealthy.Api.Pantry.Interfaces.Rest.Resources;

namespace Mealthy.Api.Pantry.Interfaces.Rest.Mappings;

/// <summary>Mapeo manual Resource ↔ Command / Entity ↔ Resource (sin AutoMapper).</summary>
public static class PantryMappings
{
    public static SupplyCommand ToCommand(this SupplyRequest r) => new(r.Name, r.Quantity, r.Unit, r.ExpiresOn);

    public static SupplyResponse ToResponse(this Supply s) =>
        new(s.Id, s.Name, s.Quantity, s.Unit, s.ExpiresOn, s.CreatedAt, s.UpdatedAt, s.Version);
}
