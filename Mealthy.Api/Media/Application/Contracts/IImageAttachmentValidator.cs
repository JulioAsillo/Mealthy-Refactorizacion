namespace Mealthy.Api.Media.Application.Contracts;

/// <summary>
/// Contrato PÚBLICO para los demás contextos: valida que un usuario pueda asociar una imagen
/// a su receta, tienda o producto.
/// </summary>
public interface IImageAttachmentValidator
{
    /// <summary>
    /// No hace nada si <paramref name="imageId"/> es null. Lanza NotFound, Forbidden (no es suya)
    /// o BusinessRule (aún no subida o fallida).
    /// </summary>
    Task EnsureAttachableAsync(int? imageId, int userId, CancellationToken ct = default);
}
