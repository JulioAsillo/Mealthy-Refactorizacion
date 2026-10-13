using Mealthy.Api.Media.Domain.Model;

namespace Mealthy.Api.Media.Interfaces.Rest.Resources;

public record ImageResponse(
    int Id,
    ImageStatus Status,
    int? Width,
    int? Height,
    string? FailureReason,
    DateTime CreatedAt);

/// <summary>
/// Instrucciones para el PUT directo a Blob Storage: el cliente envía el archivo a <see cref="UploadUrl"/>
/// con EXACTAMENTE estas cabeceras, sin el header Authorization de la API.
/// </summary>
public record ImageUploadResponse(
    int Id,
    string UploadUrl,
    IReadOnlyDictionary<string, string> UploadHeaders,
    DateTimeOffset ExpiresAt);
