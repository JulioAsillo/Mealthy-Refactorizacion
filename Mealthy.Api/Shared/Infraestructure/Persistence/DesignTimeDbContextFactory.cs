using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Mealthy.Api.Shared.Infraestructure.Persistence;

/// <summary>
/// Solo para "dotnet ef migrations add/remove": crea el DbContext sin arrancar Program.cs.
/// Con Aspire la connection string real solo existe mientras corre el AppHost, y las integraciones
/// (Blob/Queue Storage) no deben ser requisito para generar una migración. Generar una migración no
/// necesita conectarse; aplicarla lo hace la propia API al arrancar en Development (DatabaseMigrator).
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=mealthy_design_time")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options);
    }
}
