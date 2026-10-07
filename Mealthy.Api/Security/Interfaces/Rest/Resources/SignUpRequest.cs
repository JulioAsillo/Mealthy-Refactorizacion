using System.ComponentModel.DataAnnotations;
using Mealthy.Api.Security.Domain.Model;

namespace Mealthy.Api.Security.Interfaces.Rest.Resources;

public record SignUpRequest(
    [Required, StringLength(30, MinimumLength = 3), RegularExpression("^[a-zA-Z0-9_.]+$")] string Username,
    [Required, EmailAddress, StringLength(254)] string Email,
    [Required, StringLength(72, MinimumLength = 8)] string Password,
    [Required, StringLength(80)] string FirstName,
    [Required, StringLength(80)] string LastName,
    [Phone, StringLength(20)] string? Phone,
    DateOnly? BirthDate,
    [Required] Role Role);
