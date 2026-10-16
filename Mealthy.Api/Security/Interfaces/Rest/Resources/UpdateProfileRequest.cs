namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Phone,
    DateOnly? BirthDate);
