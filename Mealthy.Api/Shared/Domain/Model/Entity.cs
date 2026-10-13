namespace Mealthy.Api.Shared.Domain.Model;

public abstract class Entity
{
    // Campo sin propiedad pública: EF Core no lo mapea (no es columna ni navegación)
    private readonly List<IDomainEvent> _domainEvents = [];

    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    /// <summary>Registra un evento; AppDbContext lo guarda en el outbox al hacer SaveChanges.</summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Devuelve y vacía los eventos pendientes. Método (no propiedad) para que EF no intente mapearlo.</summary>
    public IReadOnlyList<IDomainEvent> DequeueDomainEvents()
    {
        if (_domainEvents.Count == 0) return [];

        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }
}
