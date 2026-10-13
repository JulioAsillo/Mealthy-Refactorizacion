using Mealthy.Api.Media.Domain.Events;
using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Media.Domain.Model;

/// <summary>
/// Metadatos de una imagen subida. El binario NUNCA vive en PostgreSQL: está en Blob Storage
/// (contenedor "images", una carpeta por Key con sus variantes WebP).
/// </summary>
public class Image : Entity
{
    /// <summary>Identificador público del blob. Guid v7: no adivinable y ordenable por fecha.</summary>
    public Guid Key { get; private set; }

    /// <summary>
    /// Usuario que la subió. Sin FK a propósito: si se borra el usuario, la fila sigue existiendo
    /// y el worker de limpieza borra fila + blobs. Con ON DELETE CASCADE los blobs quedarían huérfanos.
    /// </summary>
    public int OwnerId { get; private set; }

    /// <summary>Tipo declarado por el cliente. Solo informativo: el worker valida el contenido real.</summary>
    public string ContentType { get; private set; } = null!;
    public long DeclaredSizeBytes { get; private set; }

    public ImageStatus Status { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }

    /// <summary>Suma del peso de las variantes WebP generadas.</summary>
    public long? StoredSizeBytes { get; private set; }

    public string? FailureReason { get; private set; }
    public DateTime? UploadedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // Requerido por EF Core
    private Image() { }

    public Image(int ownerId, string contentType, long declaredSizeBytes)
    {
        if (declaredSizeBytes <= 0)
            throw new BusinessRuleException("El tamaño del archivo debe ser mayor a 0.");

        Key = Guid.CreateVersion7();
        OwnerId = ownerId;
        ContentType = contentType;
        DeclaredSizeBytes = declaredSizeBytes;
        Status = ImageStatus.AwaitingUpload;
    }

    public bool IsOwnedBy(int userId) => OwnerId == userId;

    /// <summary>Solo imágenes ya subidas pueden asociarse a una receta, tienda o producto.</summary>
    public bool IsAttachable => Status is ImageStatus.Processing or ImageStatus.Ready;

    /// <summary>Devuelve false si ya estaba subida (llamada repetida = idempotente, no se re-encola).</summary>
    public bool MarkUploaded(DateTime now)
    {
        if (Status != ImageStatus.AwaitingUpload) return false;

        Status = ImageStatus.Processing;
        UploadedAt = now;
        RaiseDomainEvent(new ImageUploaded(Id));   // el outbox lo lleva a la cola en la misma transacción
        return true;
    }

    public void MarkReady(int width, int height, long storedSizeBytes, DateTime now)
    {
        if (Status != ImageStatus.Processing)
            throw new BusinessRuleException($"No se puede completar una imagen en estado {Status}.");

        Status = ImageStatus.Ready;
        Width = width;
        Height = height;
        StoredSizeBytes = storedSizeBytes;
        ProcessedAt = now;
        FailureReason = null;
    }

    public void MarkFailed(string reason, DateTime now)
    {
        Status = ImageStatus.Failed;
        FailureReason = reason.Length <= 300 ? reason : reason[..300];
        ProcessedAt = now;
    }
}
