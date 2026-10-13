using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Mealthy.Api.Media.Application;
using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Media.Domain.Model;
using Microsoft.Extensions.Options;

namespace Mealthy.Api.Media.Infraestructure.Storage;

/// <summary>
/// Blob Storage (Azurite en local). Estructura:
///   uploads/{key}            original privado, se borra tras procesarse
///   images/{key}/{sm|md|lg}.webp   variantes públicas e inmutables (cache 1 año)
/// </summary>
public sealed class AzureBlobImageStorage(BlobServiceClient blobs, IOptions<MediaOptions> options) : IImageStorage
{
    private const string WebpContentType = "image/webp";
    private const string ImmutableCacheControl = "public, max-age=31536000, immutable";

    private BlobContainerClient Uploads => blobs.GetBlobContainerClient(options.Value.UploadsContainer);
    private BlobContainerClient Images => blobs.GetBlobContainerClient(options.Value.ImagesContainer);

    private BlobClient UploadBlob(Guid key) => Uploads.GetBlobClient(key.ToString("N"));
    private BlobClient VariantBlob(Guid key, ImageVariant v) => Images.GetBlobClient($"{key:N}/{v.Name}.webp");

    public async Task<UploadTarget> CreateUploadTargetAsync(Guid key, string contentType, DateTimeOffset expiresOn,
        CancellationToken ct = default)
    {
        var blob = UploadBlob(key);

        // Solo "Create": permite un único PUT de un blob nuevo; no se puede sobrescribir ni leer.
        var sas = new BlobSasBuilder(BlobSasPermissions.Create, expiresOn)
        {
            BlobContainerName = blob.BlobContainerName,
            BlobName = blob.Name,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1)   // tolera desfase de reloj
        };

        Uri url;
        if (blob.CanGenerateSasUri)
        {
            // Azurite / cuenta con access key (connection string)
            url = blob.GenerateSasUri(sas);
        }
        else
        {
            // Azure real con Managed Identity (sin access key): SAS de delegación de usuario
            var delegationKey = await blobs.GetUserDelegationKeyAsync(sas.StartsOn, expiresOn, ct);
            url = new BlobUriBuilder(blob.Uri)
            {
                Sas = sas.ToSasQueryParameters(delegationKey.Value, blobs.AccountName)
            }.ToUri();
        }

        return new UploadTarget(url, new Dictionary<string, string>
        {
            ["x-ms-blob-type"] = "BlockBlob",
            ["Content-Type"] = contentType
        });
    }

    public async Task<long?> GetUploadedSizeAsync(Guid key, CancellationToken ct = default)
    {
        try
        {
            var props = await UploadBlob(key).GetPropertiesAsync(cancellationToken: ct);
            return props.Value.ContentLength;
        }
        catch (RequestFailedException e) when (e.Status == StatusCodes.Status404NotFound)
        {
            return null;
        }
    }

    public async Task<Stream?> OpenUploadAsync(Guid key, long maxBytes, CancellationToken ct = default)
    {
        var size = await GetUploadedSizeAsync(key, ct);
        if (size is null) return null;
        if (size > maxBytes) throw new InvalidImageException("El archivo supera el tamaño permitido.");

        // En memoria (≤ maxBytes): ImageSharp necesita un stream con Seek para identificar y luego decodificar.
        var buffer = new MemoryStream(capacity: (int)size.Value);
        await UploadBlob(key).DownloadToAsync(buffer, ct);
        buffer.Position = 0;
        return buffer;
    }

    public async Task SaveVariantAsync(Guid key, ImageVariant variant, Stream content, CancellationToken ct = default)
        => await VariantBlob(key, variant).UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = WebpContentType,
                CacheControl = ImmutableCacheControl
            }
        }, ct);

    public async Task DeleteUploadAsync(Guid key, CancellationToken ct = default)
        => await UploadBlob(key).DeleteIfExistsAsync(cancellationToken: ct);

    public async Task DeleteAllAsync(Guid key, CancellationToken ct = default)
    {
        await DeleteUploadAsync(key, ct);
        foreach (var variant in ImageVariant.All)
            await VariantBlob(key, variant).DeleteIfExistsAsync(cancellationToken: ct);
    }

    public Uri GetVariantUri(Guid key, ImageVariant variant) => VariantBlob(key, variant).Uri;
}
