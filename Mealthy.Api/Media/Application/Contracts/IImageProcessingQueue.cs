namespace Mealthy.Api.Media.Application.Contracts;

/// <summary>Cola de trabajo para el worker que genera las variantes (Azure Storage Queue / Azurite).</summary>
public interface IImageProcessingQueue
{
    Task EnqueueAsync(int imageId, CancellationToken ct = default);
}
