namespace Mealthy.Api.Security.Application.Results;

/// <summary>Token = lo que recibe el cliente. Hash = lo que se guarda en BD.</summary>
public record GeneratedRefreshToken(string Token, string Hash, DateTime ExpiresAt);
