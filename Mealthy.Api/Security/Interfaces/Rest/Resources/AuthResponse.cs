namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserResponse User);