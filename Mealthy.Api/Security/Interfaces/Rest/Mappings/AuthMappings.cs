using Mealthy.Api.Security.Application.Commands;
using Mealthy.Api.Security.Application.Results;
using Mealthy.Api.Security.Interfaces.Rest.Resources;

namespace Mealthy.Api.Security.Interfaces.Rest.Mappings;

public static class AuthMappings
{
    public static SignUpCommand ToCommand(this SignUpRequest r) =>
        new(r.Username, r.Email, r.Password, r.FirstName, r.LastName, r.Phone, r.BirthDate, r.Role);

    public static SignInCommand ToCommand(this SignInRequest r) => new(r.Email, r.Password);

    public static RefreshTokenCommand ToCommand(this RefreshTokenRequest r) => new(r.RefreshToken);

    public static AuthResponse ToResponse(this AuthResult r) =>
        new(r.Token.Token, r.Token.ExpiresAt, r.RefreshToken, r.RefreshTokenExpiresAt, r.User.ToResponse());
}
