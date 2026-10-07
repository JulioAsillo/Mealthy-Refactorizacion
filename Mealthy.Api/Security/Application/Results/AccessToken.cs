namespace Mealthy.Api.Security.Application.Results;

public record AccessToken(string Token, DateTime ExpiresAt);
