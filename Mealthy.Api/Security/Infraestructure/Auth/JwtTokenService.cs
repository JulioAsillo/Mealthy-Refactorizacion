using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Mealthy.Api.Security.Application.Contracts;
using Mealthy.Api.Security.Application.Results;
using Mealthy.Api.Security.Domain.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Mealthy.Api.Security.Infraestructure.Auth;

public class JwtTokenService(IOptions<JwtOptions> options, TimeProvider clock) : ITokenService
{
    private readonly JwtOptions _opt = options.Value;
    private static readonly JsonWebTokenHandler Handler = new();

    public AccessToken CreateToken(User user)
    {
        var expires = clock.GetUtcNow().UtcDateTime.AddMinutes(_opt.ExpirationMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _opt.Issuer,
            Audience = _opt.Audience,
            Expires = expires,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(SecurityClaims.Role, user.Role.ToString())
            ])
        };

        return new AccessToken(Handler.CreateToken(descriptor), expires);
    }

    public GeneratedRefreshToken CreateRefreshToken()
    {
        // 64 bytes criptográficamente aleatorios → no adivinable, no es un JWT
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        var expires = clock.GetUtcNow().UtcDateTime.AddDays(_opt.RefreshTokenDays);
        return new GeneratedRefreshToken(token, HashRefreshToken(token), expires);
    }

    public string HashRefreshToken(string token)
        => Base64UrlEncoder.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
