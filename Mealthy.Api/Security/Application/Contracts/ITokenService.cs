using Mealthy.Api.Security.Application.Results;
using Mealthy.Api.Security.Domain.Model;

namespace Mealthy.Api.Security.Application.Contracts;

public interface ITokenService
{
    /// <summary>Access token JWT de vida corta.</summary>
    AccessToken CreateToken(User user);

    /// <summary>Refresh token opaco (aleatorio) + su hash para guardar en BD + vencimiento.</summary>
    GeneratedRefreshToken CreateRefreshToken();

    /// <summary>Hash SHA-256 de un refresh token recibido del cliente, para buscarlo en BD.</summary>
    string HashRefreshToken(string token);
}
