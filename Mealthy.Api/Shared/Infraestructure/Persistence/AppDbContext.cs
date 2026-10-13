using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Shared.Infraestructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Índices de trigramas: hacen que ILIKE '%texto%' use índice (GIN) en vez de recorrer la tabla
        builder.HasPostgresExtension("pg_trgm");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var entries = ChangeTracker.Entries<Entity>().ToList();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = now;
            if (entry.State == EntityState.Modified) entry.Entity.UpdatedAt = now;
        }

        // Outbox transaccional: los eventos se guardan en el MISMO SaveChanges (misma transacción)
        // que el cambio que los produjo. O se guardan ambos, o ninguno.
        var outbox = entries
            .SelectMany(e => e.Entity.DequeueDomainEvents())
            .Select(domainEvent => OutboxMessage.From(domainEvent, now))
            .ToList();

        if (outbox.Count > 0)
            Set<OutboxMessage>().AddRange(outbox);

        return base.SaveChangesAsync(ct);
    }
}