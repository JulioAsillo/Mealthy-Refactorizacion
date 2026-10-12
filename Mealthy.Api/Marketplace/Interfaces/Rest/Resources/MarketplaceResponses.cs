namespace Mealthy.Api.Marketplace.Interfaces.Rest.Resources;

public record StoreResponse(
    int Id,
    string Name,
    string Description,
    string Ruc,
    string? PhotoUrl,
    int OwnerId,
    DateTime CreatedAt);

public record ProductResponse(
    int Id,
    int StoreId,
    string Name,
    string Category,
    decimal Price,
    string Unit,
    int Stock,
    string? PhotoUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
