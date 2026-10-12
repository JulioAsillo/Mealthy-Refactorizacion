namespace Mealthy.Api.Marketplace.Application.Commands;

public record CreateStoreCommand(string Name, string Description, string Ruc, string? PhotoUrl);

public record UpdateStoreCommand(string Name, string Description, string? PhotoUrl);

public record ProductCommand(string Name, string Category, decimal Price, string Unit, int Stock, string? PhotoUrl);
