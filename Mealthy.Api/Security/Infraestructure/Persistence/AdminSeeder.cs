using Mealthy.Api.Security.Application.Contracts;
using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Security.Domain.Repositories;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Security.Infraestructure.Persistence;

/// <summary>Credenciales del Admin inicial. Se leen de user-secrets / variables de entorno, NUNCA de appsettings.</summary>
public sealed class AdminSeedOptions
{
    public const string Section = "Seed:Admin";
    public const int MinPasswordLength = 12;

    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
    public string FirstName { get; init; } = "Admin";
    public string LastName { get; init; } = "Mealthy";
}

/// <summary>
/// Crea el primer Admin al arrancar, solo si todavía no existe ninguno (idempotente).
/// El sign-up público sigue rechazando Role.Admin.
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAdminAsync(this WebApplication app, CancellationToken ct = default)
    {
        var opts = app.Configuration.GetSection(AdminSeedOptions.Section).Get<AdminSeedOptions>();

        if (opts is not
            {
                Username: { Length: > 0 } username,
                Email: { Length: > 0 } email,
                Password: { Length: >= AdminSeedOptions.MinPasswordLength } password
            })
        {
            app.Logger.LogInformation(
                "Seed de Admin omitido: configura {Section}:Username/Email/Password (mín. {Min} caracteres).",
                AdminSeedOptions.Section, AdminSeedOptions.MinPasswordLength);
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<AppDbContext>();
        if (await db.Set<User>().AsNoTracking().AnyAsync(u => u.Role == Role.Admin, ct))
            return;

        var users = sp.GetRequiredService<IUserRepository>();
        if (await users.ExistsByEmailAsync(email, ct) || await users.ExistsByUsernameAsync(username, ct))
        {
            app.Logger.LogWarning("Seed de Admin omitido: el email o username ya pertenecen a otro usuario.");
            return;
        }

        var hasher = sp.GetRequiredService<IPasswordHasher>();
        var unitOfWork = sp.GetRequiredService<IUnitOfWorks>();

        var admin = new User(username, email, hasher.Hash(password),
            opts.FirstName, opts.LastName, phone: null, birthDate: null, Role.Admin);

        await users.AddAsync(admin, ct);
        await unitOfWork.CompleteAsync(ct);

        app.Logger.LogInformation("Usuario Admin inicial creado: {Email}", User.NormalizeEmail(email));
    }
}
