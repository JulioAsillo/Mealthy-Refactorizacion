using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Shared.Infraestructure.Persistence;

public static class DatabaseMigrator
{
    /// <summary>
    /// Aplica las migraciones pendientes al arrancar. SOLO en Development: con Aspire el puerto de
    /// Postgres es dinámico, así que "dotnet ef database update" desde la consola ya no es práctico.
    /// En otros entornos las migraciones se aplican de forma explícita (bundle o script), nunca al arrancar.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app, CancellationToken ct = default)
    {
        if (!app.Environment.IsDevelopment()) return;

        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pending = (await db.Database.GetPendingMigrationsAsync(ct)).ToList();
        if (pending.Count == 0) return;

        app.Logger.LogInformation("Aplicando {Count} migración(es): {Migrations}", pending.Count, string.Join(", ", pending));
        await db.Database.MigrateAsync(ct);
    }
}
