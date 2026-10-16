using System.Linq.Expressions;
using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Mealthy.Api.Shared.Infraestructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        // Las cascadas se resuelven al guardar, no al llamar a Remove(): así un borrado lógico
        // (ISoftDeletable) no arrastra el borrado físico de sus hijos ya cargados (ingredientes, pasos).
        ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Índices de trigramas: hacen que ILIKE '%texto%' use índice (GIN) en vez de recorrer la tabla
        builder.HasPostgresExtension("pg_trgm");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Convenciones transversales: se declaran una vez para todas las entidades que las usan
        foreach (var clrType in builder.Model.GetEntityTypes().Select(t => t.ClrType).ToList())
        {
            // Concurrencia optimista con la columna de sistema xmin (no se crea: PostgreSQL ya la tiene)
            if (typeof(IVersioned).IsAssignableFrom(clrType))
                builder.Entity(clrType).Property(nameof(IVersioned.Version))
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .IsRowVersion();

            // Borrado lógico: las filas con deleted_at no existen para ninguna consulta
            if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
                builder.Entity(clrType).HasQueryFilter(NotDeleted(clrType));
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var entries = ChangeTracker.Entries<Entity>().ToList();

        foreach (var entry in entries)
        {
            if (entry is { State: EntityState.Deleted, Entity: ISoftDeletable softDeletable })
                MarkAsSoftDeleted(entry, softDeletable, now);

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

    /// <summary>DELETE → UPDATE solo de deleted_at (y updated_at). Sigue validando xmin si la entidad es IVersioned.</summary>
    private static void MarkAsSoftDeleted(EntityEntry<Entity> entry, ISoftDeletable entity, DateTime now)
    {
        entry.State = EntityState.Unchanged;
        entity.DeletedAt = now;
        entry.Property(nameof(ISoftDeletable.DeletedAt)).IsModified = true;
    }

    /// <summary>e => e.DeletedAt == null, construido para el tipo concreto.</summary>
    private static LambdaExpression NotDeleted(Type clrType)
    {
        var e = Expression.Parameter(clrType, "e");
        var deletedAt = Expression.Property(e, nameof(ISoftDeletable.DeletedAt));
        return Expression.Lambda(Expression.Equal(deletedAt, Expression.Constant(null, typeof(DateTime?))), e);
    }
}
