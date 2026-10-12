using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Pantry.Interfaces.Rest.Resources;

public record SupplyRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Range(0, 99999999.99)] decimal Quantity,
    [Required, StringLength(20, MinimumLength = 1)] string Unit,
    DateOnly? ExpiresOn);
