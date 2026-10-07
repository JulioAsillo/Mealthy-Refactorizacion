using Mealthy.Api.Security.Domain.Model;

namespace Mealthy.Api.Security.Application.Results;

public record AuthResult(User User, AccessToken Token);
