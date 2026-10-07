using Mealthy.Api.Security.Application.Results;
using Mealthy.Api.Security.Domain.Model;

namespace Mealthy.Api.Security.Application.Contracts;

public interface ITokenService
{
    AccessToken CreateToken(User user);
}
