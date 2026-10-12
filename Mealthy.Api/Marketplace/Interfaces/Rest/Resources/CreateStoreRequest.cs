using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Marketplace.Interfaces.Rest.Resources;

public record CreateStoreRequest(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    [Required, StringLength(1000)] string Description,
    // RUC peruano: 11 dígitos, empieza en 10 (persona natural), 15, 17 o 20 (persona jurídica)
    [Required, RegularExpression(@"^(10|15|17|20)\d{9}$", ErrorMessage = "RUC inválido: deben ser 11 dígitos y empezar en 10, 15, 17 o 20.")]
    string Ruc,
    [Url, StringLength(500)] string? PhotoUrl);
