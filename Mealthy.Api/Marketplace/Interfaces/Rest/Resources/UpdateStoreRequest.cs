namespace Mealthy.Api.Marketplace.Interfaces.Rest.Resources;

/// <summary>El RUC no se puede cambiar una vez registrada la tienda.</summary>
public record UpdateStoreRequest(
    string Name,
    string Description,
    int? ImageId);
