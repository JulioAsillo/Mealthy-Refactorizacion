namespace Mealthy.Api.Pantry.Interfaces.Rest.Resources;

public record SupplyRequest(
    string Name,
    decimal Quantity,
    string Unit,
    DateOnly? ExpiresOn);
