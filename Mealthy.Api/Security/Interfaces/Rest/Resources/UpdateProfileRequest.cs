using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

public record UpdateProfileRequest(
    [Required, StringLength(80)] string FirstName,
    [Required, StringLength(80)] string LastName,
    [Phone, StringLength(20)] string? Phone,
    DateOnly? BirthDate);
