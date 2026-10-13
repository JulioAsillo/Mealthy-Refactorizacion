using System.Text.Json;
using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Shared.Infraestructure.Messaging;

/// <summary>
/// Fila del outbox transaccional. No hereda de Entity: es infraestructura, no dominio
/// (y así no puede generar eventos ella misma).
/// </summary>
public sealed class OutboxMessage
{
    public const int MaxErrorLength = 2000;

    public long Id { get; private set; }

    /// <summary>Nombre completo del tipo del evento: clave para encontrar sus handlers.</summary>
    public string Type { get; private set; } = null!;

    /// <summary>Evento serializado (columna jsonb).</summary>
    public string Payload { get; private set; } = null!;

    public DateTime OccurredAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    // Requerido por EF Core
    private OutboxMessage() { }

    public static OutboxMessage From(IDomainEvent domainEvent, DateTime occurredAt)
    {
        var type = domainEvent.GetType();
        return new OutboxMessage
        {
            Type = type.FullName!,
            Payload = JsonSerializer.Serialize(domainEvent, type),
            OccurredAt = occurredAt
        };
    }

    public void MarkProcessed(DateTime now)
    {
        ProcessedAt = now;
        LastError = null;
    }

    public void RecordFailure(string error)
    {
        Attempts++;
        LastError = error.Length <= MaxErrorLength ? error : error[..MaxErrorLength];
    }
}
