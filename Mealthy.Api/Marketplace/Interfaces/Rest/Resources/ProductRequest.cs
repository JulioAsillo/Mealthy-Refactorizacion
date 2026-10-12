using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Marketplace.Interfaces.Rest.Resources;

public record ProductRequest(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, StringLength(50, MinimumLength = 2)] string Category,
    [Range(0.01, 99999999.99)] decimal Price,
    [Required, StringLength(20, MinimumLength = 1)] string Unit,
    [Range(0, int.MaxValue)] int Stock,
    [Url, StringLength(500)] string? PhotoUrl);
