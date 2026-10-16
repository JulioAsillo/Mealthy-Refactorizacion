namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

public record SignInRequest(
    string Email,
    string Password);
