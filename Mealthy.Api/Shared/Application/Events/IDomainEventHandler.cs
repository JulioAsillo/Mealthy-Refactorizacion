using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Shared.Application.Events;

/// <summary>
/// Reacción a un evento de dominio, ejecutada por el OutboxProcessor (fuera de la petición HTTP).
///
/// Entrega at-least-once: si el proceso cae a mitad, el mensaje se reintenta y el handler puede
/// ejecutarse dos veces. Todo handler DEBE ser idempotente (p. ej. comprobar el estado antes de actuar).
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
}
