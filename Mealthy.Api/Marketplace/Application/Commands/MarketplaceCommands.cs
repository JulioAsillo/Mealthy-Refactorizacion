namespace Mealthy.Api.Marketplace.Application.Commands;

public record CreateStoreCommand(string Name, string Description, string Ruc, int? ImageId);

public record UpdateStoreCommand(string Name, string Description, int? ImageId);

public record ProductCommand(string Name, string Category, decimal Price, string Unit, int Stock, int? ImageId);
