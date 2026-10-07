using Mealthy.Api.Security.Domain.Model;

namespace Mealthy.Api.Security.Application.Commands;

public record SignUpCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Phone,
    DateOnly? BirthDate,
    Role Role);
