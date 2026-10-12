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
    IRefreshTokenRepository refreshTokens,
    IUnitOfWorks unitOfWork,
    IPasswordHasher hasher,
    ITokenService tokens,
    TimeProvider clock)
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
        await unitOfWork.CompleteAsync(ct);   // primero el usuario: el refresh token necesita su Id

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResult> SignInAsync(SignInCommand cmd, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(cmd.Email, ct);

        // Mismo mensaje en ambos casos: no revelar qué emails existen.
        if (user is null || !hasher.Verify(cmd.Password, user.PasswordHash))
            throw new AuthenticationFailedException("Credenciales inválidas.");

        return await IssueTokensAsync(user, ct);
    }

    /// <summary>
    /// Rotación: el refresh token usado se revoca y se emite uno nuevo.
    /// Si llega un token YA revocado, alguien lo robó y lo reutilizó → se revocan todas
    /// las sesiones del usuario (detección de reutilización).
    /// </summary>
    public async Task<AuthResult> RefreshAsync(RefreshTokenCommand cmd, CancellationToken ct = default)
    {
        var now = Now();
        var stored = await refreshTokens.FindByHashAsync(tokens.HashRefreshToken(cmd.RefreshToken), ct)
                     ?? throw new AuthenticationFailedException("Refresh token inválido.");

        if (stored.RevokedAt is not null)
        {
            await RevokeAllAsync(stored.UserId, now, ct);
            throw new AuthenticationFailedException("Refresh token reutilizado. Inicia sesión de nuevo.");
        }

        if (!stored.IsActive(now))
            throw new AuthenticationFailedException("Refresh token expirado. Inicia sesión de nuevo.");

        var user = await users.FindByIdAsync(stored.UserId, ct)
                   ?? throw new AuthenticationFailedException("Refresh token inválido.");

        var next = tokens.CreateRefreshToken();
        stored.Revoke(now, replacedByTokenHash: next.Hash);
        await refreshTokens.AddAsync(new RefreshToken(user.Id, next.Hash, next.ExpiresAt), ct);
        await unitOfWork.CompleteAsync(ct);

        return new AuthResult(user, tokens.CreateToken(user), next.Token, next.ExpiresAt);
    }

    /// <summary>Cierra la sesión revocando el refresh token. Idempotente: un token desconocido no da error.</summary>
    public async Task SignOutAsync(RefreshTokenCommand cmd, CancellationToken ct = default)
    {
        var stored = await refreshTokens.FindByHashAsync(tokens.HashRefreshToken(cmd.RefreshToken), ct);
        if (stored is null) return;

        stored.Revoke(Now());
        await unitOfWork.CompleteAsync(ct);
    }

    // ---------- Privados ----------

    private async Task<AuthResult> IssueTokensAsync(User user, CancellationToken ct)
    {
        var refresh = tokens.CreateRefreshToken();
        await refreshTokens.AddAsync(new RefreshToken(user.Id, refresh.Hash, refresh.ExpiresAt), ct);
        await unitOfWork.CompleteAsync(ct);

        return new AuthResult(user, tokens.CreateToken(user), refresh.Token, refresh.ExpiresAt);
    }

    private async Task RevokeAllAsync(int userId, DateTime now, CancellationToken ct)
    {
        foreach (var token in await refreshTokens.ListActiveByUserAsync(userId, now, ct))
            token.Revoke(now);
        await unitOfWork.CompleteAsync(ct);
    }

    private DateTime Now() => clock.GetUtcNow().UtcDateTime;
}
