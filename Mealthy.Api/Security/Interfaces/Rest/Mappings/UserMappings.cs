using Mealthy.Api.Security.Application.Commands;
using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Security.Interfaces.Rest.Resources;

namespace Mealthy.Api.Security.Interfaces.Rest.Mappings;

/// <summary>Mapeo manual Resource ↔ Command / Entity ↔ Resource (sin AutoMapper).</summary>
public static class UserMappings
{
    public static UserResponse ToResponse(this User u) =>
        new(u.Id, u.Username, u.Email, u.FirstName, u.LastName, u.Phone, u.BirthDate, u.Role, u.CreatedAt);

    public static UpdateProfileCommand ToCommand(this UpdateProfileRequest r) =>
        new(r.FirstName, r.LastName, r.Phone, r.BirthDate);
}
