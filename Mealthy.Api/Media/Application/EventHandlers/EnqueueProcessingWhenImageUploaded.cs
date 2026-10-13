using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Media.Domain.Events;
using Mealthy.Api.Shared.Application.Events;

namespace Mealthy.Api.Media.Application.EventHandlers;

/// <summary>
/// Outbox → cola de procesamiento. Idempotente: si el mensaje se encola dos veces, el worker
/// descarta el segundo porque la imagen ya no está en Processing.
/// </summary>
public sealed class EnqueueProcessingWhenImageUploaded(IImageProcessingQueue queue)
    : IDomainEventHandler<ImageUploaded>
{
    public Task HandleAsync(ImageUploaded domainEvent, CancellationToken ct = default)
        => queue.EnqueueAsync(domainEvent.ImageId, ct);
}
