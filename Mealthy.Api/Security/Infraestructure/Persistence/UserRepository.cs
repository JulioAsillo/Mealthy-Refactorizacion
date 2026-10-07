using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Security.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Security.Infraestructure.Persistence;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = User.NormalizeEmail(email);
        return Set.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = User.NormalizeEmail(email);
        return Set.AnyAsync(u => u.Email == normalized, ct);
    }

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
        => Set.AnyAsync(u => u.Username == username.Trim(), ct);

    public async Task<(IReadOnlyList<User> Items, int Total)> ListPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(u => u.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }
}
