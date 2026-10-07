using Mealthy.Api.Security.Application.Commands;
using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Security.Domain.Repositories;
using Mealthy.Api.Shared.Application;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Security.Application.Services;

public class UserService(IUserRepository users, IUnitOfWorks unitOfWork)
{
    public async Task<User> GetByIdAsync(int id, CancellationToken ct = default)
        => await users.FindByIdAsync(id, ct)
           ?? throw new NotFoundException($"Usuario {id} no encontrado.");

    // Recibe primitivos, no PageQuery: Application no depende de la capa Interfaces.
    public async Task<PagedResult<User>> ListAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await users.ListPagedAsync(page, pageSize, ct);
        return new PagedResult<User>(items, page, pageSize, total);
    }

    public async Task<User> UpdateProfileAsync(int id, UpdateProfileCommand cmd, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(id, ct);

        if (cmd.BirthDate is { } bd && bd > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleException("La fecha de nacimiento no puede estar en el futuro.");

        user.UpdateProfile(cmd.FirstName, cmd.LastName, cmd.Phone, cmd.BirthDate);
        await unitOfWork.CompleteAsync(ct);   // entidad trackeada: no hace falta Update()
        return user;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(id, ct);
        users.Remove(user);
        await unitOfWork.CompleteAsync(ct);
    }
}
