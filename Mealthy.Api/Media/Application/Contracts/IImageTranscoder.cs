using Mealthy.Api.Media.Domain.Model;

namespace Mealthy.Api.Media.Application.Contracts;

public sealed record TranscodedVariant(ImageVariant Variant, byte[] Content);

/// <summary>Dimensiones de la imagen ya orientada y las variantes codificadas en WebP.</summary>
public sealed record TranscodedImage(int Width, int Height, IReadOnlyList<TranscodedVariant> Variants);

/// <summary>Decodifica, valida y re-codifica. Implementación: ImageSharp.</summary>
public interface IImageTranscoder
{
    /// <exception cref="InvalidImageException">El contenido no es una imagen permitida.</exception>
    Task<TranscodedImage> TranscodeAsync(Stream source, IReadOnlyList<ImageVariant> variants,
        CancellationToken ct = default);
}
