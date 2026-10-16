namespace Mealthy.Api.Marketplace.Interfaces.Rest.Resources;

public record CreateStoreRequest(
    string Name,
    string Description,
    // RUC peruano: 11 dígitos, empieza en 10 (persona natural), 15, 17 o 20 (persona jurídica)
    string Ruc,
    // Id devuelto por POST /api/v1/images (opcional)
    int? ImageId);
