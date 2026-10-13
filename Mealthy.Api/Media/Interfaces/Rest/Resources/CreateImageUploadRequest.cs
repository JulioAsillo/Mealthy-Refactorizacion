using System.ComponentModel.DataAnnotations;

namespace Mealthy.Api.Media.Interfaces.Rest.Resources;

/// <summary>Lo que el navegador sabe del archivo ANTES de subirlo (tras comprimirlo).</summary>
public record CreateImageUploadRequest(
    [Required, StringLength(50)] string ContentType,
    [Range(1, long.MaxValue)] long SizeBytes);
