using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Marketplace.Interfaces.Rest.Resources;

/// <summary>El RUC no se puede cambiar una vez registrada la tienda.</summary>
public record UpdateStoreRequest(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [Required, StringLength(1000)] string Description,
    [Url, StringLength(500)] string? PhotoUrl);
