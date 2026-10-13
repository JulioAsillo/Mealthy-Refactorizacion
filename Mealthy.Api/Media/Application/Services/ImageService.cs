using Mealthy.Api.Media.Application.Commands;
using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Media.Application.Results;
using Mealthy.Api.Media.Domain.Model;
using Mealthy.Api.Media.Domain.Repositories;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;
using Microsoft.Extensions.Options;

namespace Mealthy.Api.Media.Application.Services;

/// <summary>
/// Casos de uso de imágenes. Flujo de subida:
///  1. POST /images            → fila AwaitingUpload + URL SAS de escritura (el binario NO pasa por la API).
///  2. PUT &lt;url SAS&gt;          → el navegador sube directo a Blob Storage.
///  3. POST /images/{id}/complete → se valida el tamaño real y se encola para el worker.
///  4. Worker                  → variantes WebP, borra el original → Ready.
/// </summary>
public class ImageService(
    IImageRepository images,
    IImageStorage storage,
    IImageProcessingQueue queue,
    IUnitOfWorks unitOfWork,
    IOptions<MediaOptions> options,
    TimeProvider clock) : IImageAttachmentValidator
{
    private const int CleanupBatchSize = 100;
    private MediaOptions Options => options.Value;

    public async Task<ImageUpload> CreateUploadAsync(int ownerId, CreateImageUploadCommand cmd, CancellationToken ct = default)
    {
        var contentType = cmd.ContentType.Trim().ToLowerInvariant();

        if (!Options.AllowedContentTypes.Contains(contentType))
            throw new BusinessRuleException($"Formato no permitido. Usa: {string.Join(", ", Options.AllowedContentTypes)}.");
        if (cmd.SizeBytes > Options.MaxUploadBytes)
            throw new BusinessRuleException($"La imagen no puede pesar más de {ToMegabytes(Options.MaxUploadBytes)} MB.");

        var image = new Image(ownerId, contentType, cmd.SizeBytes);
        await images.AddAsync(image, ct);
        await unitOfWork.CompleteAsync(ct);

        var expiresAt = clock.GetUtcNow().Add(Options.UploadUrlLifetime);
        var target = await storage.CreateUploadTargetAsync(image.Key, contentType, expiresAt, ct);
        return new ImageUpload(image, target, expiresAt);
    }

    public async Task<Image> CompleteUploadAsync(int id, int userId, CancellationToken ct = default)
    {
        var image = await images.FindByIdAsync(id, ct)
                    ?? throw new NotFoundException($"Imagen {id} no encontrada.");
        if (!image.IsOwnedBy(userId))
            throw new ForbiddenException("Solo quien subió la imagen puede confirmarla.");

        if (image.Status != ImageStatus.AwaitingUpload)
            return image;   // idempotente: un doble clic no encola dos veces

        var size = await storage.GetUploadedSizeAsync(image.Key, ct)
                   ?? throw new BusinessRuleException("El archivo todavía no se ha subido.");

        if (size > Options.MaxUploadBytes)
        {
            // El SAS no puede limitar el tamaño: se valida aquí, con el archivo ya en el storage.
            image.MarkFailed("El archivo supera el tamaño permitido.", Now);
            await unitOfWork.CompleteAsync(ct);
            await storage.DeleteUploadAsync(image.Key, ct);
            throw new BusinessRuleException($"La imagen no puede pesar más de {ToMegabytes(Options.MaxUploadBytes)} MB.");
        }

        image.MarkUploaded(Now);
        await unitOfWork.CompleteAsync(ct);

        // Ventana conocida: si el proceso cae entre el commit y el envío, la imagen queda en Processing.
        // Se resuelve con el patrón Outbox (Fase 4 del roadmap).
        await queue.EnqueueAsync(image.Id, ct);
        return image;
    }

    public async Task<Image> GetAsync(int id, CancellationToken ct = default)
        => await images.GetReadOnlyAsync(id, ct)
           ?? throw new NotFoundException($"Imagen {id} no encontrada.");

    public async Task<Uri> GetVariantUriAsync(int id, ImageVariant variant, CancellationToken ct = default)
    {
        var image = await GetAsync(id, ct);
        if (image.Status != ImageStatus.Ready)
            throw new NotFoundException($"La imagen {id} todavía no está disponible.");

        return storage.GetVariantUri(image.Key, variant);
    }

    public async Task EnsureAttachableAsync(int? imageId, int userId, CancellationToken ct = default)
    {
        if (imageId is not { } id) return;

        var image = await GetAsync(id, ct);
        if (!image.IsOwnedBy(userId))
            throw new ForbiddenException("Solo puedes usar imágenes que tú subiste.");
        if (!image.IsAttachable)
            throw new BusinessRuleException(image.Status == ImageStatus.Failed
                ? "La imagen no se pudo procesar. Sube otra."
                : "La imagen aún no termina de subirse.");
    }

    /// <summary>
    /// Borra (blobs + fila) las imágenes que nadie referencia tras el periodo de gracia:
    /// subidas abandonadas, fotos reemplazadas, fallidas y las de entidades eliminadas.
    /// </summary>
    public async Task<int> CleanupOrphansAsync(CancellationToken ct = default)
    {
        var cutoff = Now - Options.OrphanGracePeriod;
        var orphans = await images.ListOrphansAsync(cutoff, CleanupBatchSize, ct);

        foreach (var image in orphans)
        {
            await storage.DeleteAllAsync(image.Key, ct);   // primero blobs: si falla, la fila sigue y se reintenta
            images.Remove(image);
        }

        await unitOfWork.CompleteAsync(ct);
        return orphans.Count;
    }

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    private static string ToMegabytes(long bytes) => (bytes / 1024d / 1024d).ToString("0.#");
}
