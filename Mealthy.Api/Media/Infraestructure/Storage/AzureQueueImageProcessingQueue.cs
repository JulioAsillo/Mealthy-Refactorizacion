using System.Globalization;
using Azure.Storage.Queues;
using Mealthy.Api.Media.Application;
using Mealthy.Api.Media.Application.Contracts;
using Microsoft.Extensions.Options;

namespace Mealthy.Api.Media.Infraestructure.Storage;

public sealed class AzureQueueImageProcessingQueue(QueueServiceClient queues, IOptions<MediaOptions> options)
    : IImageProcessingQueue
{
    /// <summary>El mensaje solo lleva el id: el worker relee el estado actual desde la BD.</summary>
    public async Task EnqueueAsync(int imageId, CancellationToken ct = default)
        => await queues.GetQueueClient(options.Value.ProcessingQueue)
            .SendMessageAsync(imageId.ToString(CultureInfo.InvariantCulture), ct);
}
