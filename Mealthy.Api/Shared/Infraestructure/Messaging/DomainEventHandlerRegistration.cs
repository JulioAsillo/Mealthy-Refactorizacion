using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Shared.Infraestructure.Messaging;

/// <summary>
/// Une un tipo de evento con la forma tipada de invocar su handler.
/// Se crea en <see cref="OutboxExtensions.AddDomainEventHandler{TEvent,THandler}"/>: así el processor no usa reflexión.
/// </summary>
public sealed record DomainEventHandlerRegistration(
    Type EventType,
    Func<IServiceProvider, IDomainEvent, CancellationToken, Task> Invoke);
