using Mealthy.Api.Security.Application.Commands;
using Mealthy.Api.Security.Application.Contracts;
using Mealthy.Api.Security.Application.Results;
using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Security.Domain.Repositories;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Security.Application.Services;

public class AuthService(
    IUserRepository users,
    IUnitOfWorks unitOfWork,
    IPasswordHasher hasher,
    ITokenService tokens)
{
    public async Task<AuthResult> SignUpAsync(SignUpCommand cmd, CancellationToken ct = default)
    {
        // Admin nunca se auto-registra: se asigna manualmente.
        if (cmd.Role == Role.Admin)
            throw new BusinessRuleException("No es posible registrarse con el rol Admin.");

        if (await users.ExistsByEmailAsync(cmd.Email, ct))
            throw new ConflictException("El email ya está registrado.");
        if (await users.ExistsByUsernameAsync(cmd.Username, ct))
            throw new ConflictException("El nombre de usuario ya está en uso.");

        if (cmd.BirthDate is { } bd && bd > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleException("La fecha de nacimiento no puede estar en el futuro.");

        var user = new User(cmd.Username, cmd.Email, hasher.Hash(cmd.Password),
            cmd.FirstName, cmd.LastName, cmd.Phone, cmd.BirthDate, cmd.Role);

        await users.AddAsync(user, ct);
        await unitOfWork.CompleteAsync(ct);   // el índice único es la red de seguridad ante carreras

        return new AuthResult(user, tokens.CreateToken(user));
    }

    public async Task<AuthResult> SignInAsync(SignInCommand cmd, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(cmd.Email, ct);

        // Mismo mensaje en ambos casos: no revelar qué emails existen.
        if (user is null || !hasher.Verify(cmd.Password, user.PasswordHash))
            throw new AuthenticationFailedException("Credenciales inválidas.");

        return new AuthResult(user, tokens.CreateToken(user));
    }
}
