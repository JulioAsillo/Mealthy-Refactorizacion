using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

public record RefreshTokenRequest([Required, StringLength(200)] string RefreshToken);