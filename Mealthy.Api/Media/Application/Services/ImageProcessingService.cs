using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Media.Domain.Model;
using Mealthy.Api.Media.Domain.Repositories;
using Mealthy.Api.Shared.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Mealthy.Api.Media.Application.Services;

/// <summary>Caso de uso del worker: genera las variantes WebP de una imagen en estado Processing.</summary>
public class ImageProcessingService(
    IImageRepository images,
    IImageStorage storage,
    IImageTranscoder transcoder,
    IUnitOfWorks unitOfWork,
    IOptions<MediaOptions> options,
    TimeProvider clock,
    ILogger<ImageProcessingService> logger)
{
    /// <summary>
    /// Idempotente: si la imagen ya no está en Processing (mensaje duplicado) no hace nada.
    /// Errores de contenido → Failed (sin reintento). Errores transitorios → excepción (el worker reintenta).
    /// </summary>
    public async Task ProcessAsync(int imageId, CancellationToken ct = default)
    {
        var image = await images.FindByIdAsync(imageId, ct);
        if (image is not { Status: ImageStatus.Processing })
        {
            logger.LogInformation("Imagen {ImageId} omitida: no existe o no está en Processing.", imageId);
            return;
        }

        try
        {
            await using var source = await storage.OpenUploadAsync(image.Key, options.Value.MaxUploadBytes, ct)
                                     ?? throw new InvalidImageException("El archivo original ya no existe.");

            var result = await transcoder.TranscodeAsync(source, ImageVariant.All, ct);

            foreach (var variant in result.Variants)
            {
                using var content = new MemoryStream(variant.Content, writable: false);
                await storage.SaveVariantAsync(image.Key, variant.Variant, content, ct);
            }

            var storedBytes = result.Variants.Sum(v => (long)v.Content.Length);
            image.MarkReady(result.Width, result.Height, storedBytes, clock.GetUtcNow().UtcDateTime);

            logger.LogInformation("Imagen {ImageId} lista: {Width}x{Height}, {Original} B → {Stored} B en {Count} variantes.",
                imageId, result.Width, result.Height, image.DeclaredSizeBytes, storedBytes, result.Variants.Count);
        }
        catch (InvalidImageException ex)
        {
            image.MarkFailed(ex.Message, clock.GetUtcNow().UtcDateTime);
            logger.LogWarning("Imagen {ImageId} rechazada: {Reason}", imageId, ex.Message);
        }

        await unitOfWork.CompleteAsync(ct);

        // El original no se conserva: puede traer EXIF con GPS y ocupa más que las 3 variantes juntas.
        await storage.DeleteUploadAsync(image.Key, ct);
    }

    /// <summary>Se agotaron los reintentos (error transitorio persistente).</summary>
    public async Task FailAsync(int imageId, string reason, CancellationToken ct = default)
    {
        var image = await images.FindByIdAsync(imageId, ct);
        if (image is not { Status: ImageStatus.Processing }) return;

        image.MarkFailed(reason, clock.GetUtcNow().UtcDateTime);
        await unitOfWork.CompleteAsync(ct);
    }
}
