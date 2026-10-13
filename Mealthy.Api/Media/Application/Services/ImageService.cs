using Mealthy.Api.Media.Application.Commands;
using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Media.Application.Results;
using Mealthy.Api.Media.Domain.Model;
using Mealthy.Api.Media.Domain.Repositories;
using Mealthy.Api.Shared.Application.Authorization;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace Mealthy.Api.Media.Application.Services;

/// <summary>
/// Casos de uso de imágenes. Flujo de subida:
///  1. POST /images            → fila AwaitingUpload + URL SAS de escritura (el binario NO pasa por la API).
///  2. PUT &lt;url SAS&gt;          → el navegador sube directo a Blob Storage.
///  3. POST /images/{id}/complete → se valida el tamaño real; el evento ImageUploaded va al outbox.
///  4. Worker                  → variantes WebP, borra el original → Ready.
/// </summary>
public class ImageService(
    IImageRepository images,
    IImageStorage storage,
    IResourceAuthorizer authorizer,
    IUnitOfWorks unitOfWork,
    IOptions<MediaOptions> options,
    HybridCache cache,
    TimeProvider clock) : IImageAttachmentValidator
{
    private const int CleanupBatchSize = 100;

    /// <summary>
    /// Una imagen Ready es inmutable: su Key no cambia hasta que se borra. Cada &lt;img&gt; de un listado pasa por
    /// GET /images/{id}/content, así que la Key se cachea (L1 memoria + L2 Redis) en vez de consultar la BD.
    /// </summary>
    private static readonly HybridCacheEntryOptions ReadyImageCacheOptions = new()
    {
        Expiration = TimeSpan.FromHours(6),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };
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

    public async Task<Image> CompleteUploadAsync(int id, CancellationToken ct = default)
    {
        var image = await images.FindByIdAsync(id, ct)
                    ?? throw new NotFoundException($"Imagen {id} no encontrada.");
        await authorizer.EnsureAsync(image, ResourceAction.Manage, ct);   // solo quien la subió

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

        // MarkUploaded genera ImageUploaded → outbox_messages en el MISMO commit.
        // El OutboxProcessor lo encola después: si el proceso cae, el mensaje sigue en la tabla.
        image.MarkUploaded(Now);
        await unitOfWork.CompleteAsync(ct);
        return image;
    }

    public async Task<Image> GetAsync(int id, CancellationToken ct = default)
        => await images.GetReadOnlyAsync(id, ct)
           ?? throw new NotFoundException($"Imagen {id} no encontrada.");

    public async Task<Uri> GetVariantUriAsync(int id, ImageVariant variant, CancellationToken ct = default)
    {
        // Si la imagen no existe o aún no está Ready, la factoría lanza y HybridCache NO guarda nada:
        // en cuanto el worker termine, la siguiente petición ya la encuentra.
        var key = await cache.GetOrCreateAsync(ReadyImageCacheKey(id), async token =>
        {
            var image = await GetAsync(id, token);
            if (image.Status != ImageStatus.Ready)
                throw new NotFoundException($"La imagen {id} todavía no está disponible.");
            return image.Key;
        }, ReadyImageCacheOptions, cancellationToken: ct);

        return storage.GetVariantUri(key, variant);
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
            await cache.RemoveAsync(ReadyImageCacheKey(image.Id), ct);
            images.Remove(image);
        }

        await unitOfWork.CompleteAsync(ct);
        return orphans.Count;
    }

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    private static string ReadyImageCacheKey(int id) => $"media:image:{id}:key";

    private static string ToMegabytes(long bytes) => (bytes / 1024d / 1024d).ToString("0.#");
}
