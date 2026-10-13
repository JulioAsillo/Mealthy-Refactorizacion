using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Media.Application;

/// <summary>
/// Configuración del contexto Media (sección "Media"). Los valores por defecto sirven para desarrollo;
/// no hace falta tocar appsettings salvo para cambiarlos.
/// </summary>
public sealed class MediaOptions
{
    public const string Section = "Media";

    /// <summary>Nombres de conexión que inyecta Aspire (storage.AddBlobs("blobs") / AddQueues("queues")).</summary>
    public const string BlobsConnectionName = "blobs";
    public const string QueuesConnectionName = "queues";

    /// <summary>Peso máximo del archivo original (el frontend ya lo comprime antes de subirlo).</summary>
    [Range(1, 20 * 1024 * 1024)]
    public long MaxUploadBytes { get; init; } = 5 * 1024 * 1024;

    /// <summary>Ancho × alto máximo. Frena "bombas de descompresión" (PNG pequeño que ocupa GB en RAM).</summary>
    [Range(1, 100_000_000)]
    public long MaxPixels { get; init; } = 40_000_000;

    public string[] AllowedContentTypes { get; init; } = ["image/jpeg", "image/png", "image/webp"];

    [Range(1, 100)]
    public int WebpQuality { get; init; } = 80;

    /// <summary>Vigencia de la URL SAS de subida. Corta: solo debe servir para un PUT inmediato.</summary>
    public TimeSpan UploadUrlLifetime { get; init; } = TimeSpan.FromMinutes(10);

    /// <summary>Tiempo que se espera antes de borrar una imagen que nadie referencia.</summary>
    public TimeSpan OrphanGracePeriod { get; init; } = TimeSpan.FromHours(24);

    public TimeSpan CleanupInterval { get; init; } = TimeSpan.FromHours(1);

    /// <summary>Contenedor privado de originales (se borran tras procesarse: traen EXIF/GPS).</summary>
    public string UploadsContainer { get; init; } = "uploads";

    /// <summary>Contenedor de lectura pública con las variantes WebP.</summary>
    public string ImagesContainer { get; init; } = "images";

    public string ProcessingQueue { get; init; } = "image-processing";
}
