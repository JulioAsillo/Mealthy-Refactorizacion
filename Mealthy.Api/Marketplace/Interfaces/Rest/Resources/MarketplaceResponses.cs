namespace Mealthy.Api.Marketplace.Interfaces.Rest.Resources;

public record StoreResponse(
    int Id,
    string Name,
    string Description,
    string Ruc,
    // Foto: GET /api/v1/images/{ImageId}/content?size=sm|md|lg
    int? ImageId,
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
    // Foto: GET /api/v1/images/{ImageId}/content?size=sm|md|lg
    int? ImageId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
