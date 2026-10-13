namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

/// <summary>El refresh token NO va aquí: viaja en la cookie HttpOnly "mealthy_refresh".</summary>
public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserResponse User);
