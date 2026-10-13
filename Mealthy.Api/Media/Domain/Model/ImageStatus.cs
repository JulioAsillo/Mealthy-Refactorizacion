namespace Mealthy.Api.Media.Domain.Model;

/// <summary>Ciclo de vida: AwaitingUpload → Processing → Ready | Failed.</summary>
public enum ImageStatus
{
    /// <summary>Se emitió la URL de subida; el archivo aún no llega al storage.</summary>
    AwaitingUpload,
    /// <summary>El archivo está en el storage y espera al worker (en cola).</summary>
    Processing,
    /// <summary>Variantes WebP generadas: la imagen se puede mostrar.</summary>
    Ready,
    /// <summary>No es una imagen válida o no se pudo procesar. No se puede asociar a nada.</summary>
    Failed
}
