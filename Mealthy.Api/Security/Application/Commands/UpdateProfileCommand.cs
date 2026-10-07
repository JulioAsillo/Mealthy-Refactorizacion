namespace Mealthy.Api.Security.Application.Commands;

public record UpdateProfileCommand(string FirstName, string LastName, string? Phone, DateOnly? BirthDate);
