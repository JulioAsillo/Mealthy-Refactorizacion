using Mealthy.Api.Media.Domain.Model;

namespace Mealthy.Api.Media.Application.Contracts;

/// <summary>URL firmada a la que el navegador sube el archivo con un PUT, y las cabeceras que debe enviar.</summary>
public sealed record UploadTarget(Uri Url, IReadOnlyDictionary<string, string> RequiredHeaders);

/// <summary>
/// Puerto de almacenamiento de binarios. La implementación (Azure Blob Storage / Azurite) vive en
/// Infraestructure: la capa de aplicación no conoce SAS, contenedores ni cabeceras x-ms-*.
/// </summary>
public interface IImageStorage
{
    Task<UploadTarget> CreateUploadTargetAsync(Guid key, string contentType, DateTimeOffset expiresOn,
        CancellationToken ct = default);

    /// <summary>Tamaño del original subido, o null si todavía no existe.</summary>
    Task<long?> GetUploadedSizeAsync(Guid key, CancellationToken ct = default);

    /// <summary>
    /// Original en memoria (stream posicionado en 0), o null si no existe.
    /// Lanza <see cref="InvalidImageException"/> si supera <paramref name="maxBytes"/>.
    /// </summary>
    Task<Stream?> OpenUploadAsync(Guid key, long maxBytes, CancellationToken ct = default);

    Task SaveVariantAsync(Guid key, ImageVariant variant, Stream content, CancellationToken ct = default);

    Task DeleteUploadAsync(Guid key, CancellationToken ct = default);

    /// <summary>Borra original y variantes. Idempotente.</summary>
    Task DeleteAllAsync(Guid key, CancellationToken ct = default);

    Uri GetVariantUri(Guid key, ImageVariant variant);
}
