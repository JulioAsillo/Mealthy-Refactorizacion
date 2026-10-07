using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

public record SignInRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);
