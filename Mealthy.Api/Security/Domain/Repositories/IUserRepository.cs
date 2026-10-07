using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Shared.Domain.Repositories;

namespace Mealthy.Api.Security.Domain.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, int Total)> ListPagedAsync(int page, int pageSize, CancellationToken ct = default);
}
