using Mealthy.Api.Media.Application.Commands;
using Mealthy.Api.Media.Application.Results;
using Mealthy.Api.Media.Domain.Model;
using Mealthy.Api.Media.Interfaces.Rest.Resources;

namespace Mealthy.Api.Media.Interfaces.Rest.Mappings;

/// <summary>Mapeo manual Resource ↔ Command / Entity ↔ Resource (sin AutoMapper).</summary>
public static class MediaMappings
{
    public static CreateImageUploadCommand ToCommand(this CreateImageUploadRequest r) => new(r.ContentType, r.SizeBytes);

    public static ImageResponse ToResponse(this Image i) =>
        new(i.Id, i.Status, i.Width, i.Height, i.FailureReason, i.CreatedAt);

    public static ImageUploadResponse ToResponse(this ImageUpload u) =>
        new(u.Image.Id, u.Target.Url.ToString(), u.Target.RequiredHeaders, u.ExpiresAt);
}
