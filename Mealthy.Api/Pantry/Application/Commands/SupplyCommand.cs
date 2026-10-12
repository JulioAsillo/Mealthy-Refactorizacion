namespace Mealthy.Api.Pantry.Application.Commands;

public record SupplyCommand(string Name, decimal Quantity, string Unit, DateOnly? ExpiresOn);
