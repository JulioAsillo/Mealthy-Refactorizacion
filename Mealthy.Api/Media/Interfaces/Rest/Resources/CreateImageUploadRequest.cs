namespace Mealthy.Api.Media.Interfaces.Rest.Resources;

/// <summary>Lo que el navegador sabe del archivo ANTES de subirlo (tras comprimirlo).</summary>
public record CreateImageUploadRequest(
    string ContentType,
    long SizeBytes);
