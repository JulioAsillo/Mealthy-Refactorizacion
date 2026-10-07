using Mealthy.Api.Security.Domain.Model;

namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

public record UserResponse(
    int Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    DateOnly? BirthDate,
    Role Role,
    DateTime CreatedAt);
