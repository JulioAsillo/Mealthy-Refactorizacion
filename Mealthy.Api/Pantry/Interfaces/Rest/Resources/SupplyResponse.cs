namespace Mealthy.Api.Pantry.Interfaces.Rest.Resources;

public record SupplyResponse(
    int Id,
    string Name,
    decimal Quantity,
    string Unit,
    DateOnly? ExpiresOn,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    uint Version);
