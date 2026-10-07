namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

public record AuthResponse(string Token, DateTime ExpiresAt, UserResponse User);
