namespace Mealthy.Api.Marketplace.Interfaces.Rest.Resources;

public record ProductRequest(
    string Name,
    string Category,
    decimal Price,
    string Unit,
    int Stock,
    int? ImageId);
