using Mealthy.Api.Security.Domain.Model;

namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

public record SignUpRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    DateOnly? BirthDate,
    Role Role);
