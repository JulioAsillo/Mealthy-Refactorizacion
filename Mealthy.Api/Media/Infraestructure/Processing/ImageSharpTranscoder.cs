using Mealthy.Api.Media.Application;
using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Media.Domain.Model;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SharpImage = SixLabors.ImageSharp.Image;

namespace Mealthy.Api.Media.Infraestructure.Processing;

/// <summary>
/// ImageSharp (Six Labors Split License: gratis para open source y empresas pequeñas).
/// Re-codificar SIEMPRE (nunca servir el original) neutraliza archivos "políglotas"
/// que se hacen pasar por imagen y elimina metadatos con datos personales.
/// </summary>
public sealed class ImageSharpTranscoder(IOptions<MediaOptions> options) : IImageTranscoder
{
    private static readonly DecoderOptions Decoder = new() { MaxFrames = 1 };   // GIF/WebP animados → solo el primer frame

    public async Task<TranscodedImage> TranscodeAsync(Stream source, IReadOnlyList<ImageVariant> variants,
        CancellationToken ct = default)
    {
        // 1. Identificar sin decodificar: formato real (magic bytes) y resolución
        ImageInfo info;
        try
        {
            info = await SharpImage.IdentifyAsync(Decoder, source, ct);
        }
        catch (ImageFormatException)
        {
            throw new InvalidImageException("El archivo no es una imagen válida.");
        }

        if (info.Metadata.DecodedImageFormat is not (JpegFormat or PngFormat or WebpFormat))
            throw new InvalidImageException("Formato no permitido. Usa JPG, PNG o WebP.");

        if ((long)info.Width * info.Height > options.Value.MaxPixels)
            throw new InvalidImageException("La resolución de la imagen es demasiado grande.");

        // 2. Decodificar, orientar según EXIF y quitar metadatos (GPS, cámara, autor…)
        source.Position = 0;
        using var image = await SharpImage.LoadAsync(Decoder, source, ct);
        image.Mutate(x => x.AutoOrient());
        image.Metadata.ExifProfile = null;
        image.Metadata.XmpProfile = null;
        image.Metadata.IptcProfile = null;

        // 3. Una variante WebP por tamaño (nunca se agranda una imagen pequeña)
        var encoder = new WebpEncoder
        {
            FileFormat = WebpFileFormatType.Lossy,
            Quality = options.Value.WebpQuality
        };

        var outputs = new List<TranscodedVariant>(variants.Count);
        foreach (var variant in variants)
        {
            ct.ThrowIfCancellationRequested();

            var needsResize = image.Width > variant.MaxSize || image.Height > variant.MaxSize;
            using var resized = needsResize
                ? image.Clone(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(variant.MaxSize, variant.MaxSize)
                }))
                : image.Clone(_ => { });

            using var output = new MemoryStream();
            await resized.SaveAsWebpAsync(output, encoder, ct);
            outputs.Add(new TranscodedVariant(variant, output.ToArray()));
        }

        return new TranscodedImage(image.Width, image.Height, outputs);
    }
}
