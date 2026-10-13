using Mealthy.Api.Media.Application.Services;
using Mealthy.Api.Media.Domain.Model;
using Mealthy.Api.Media.Interfaces.Rest.Mappings;
using Mealthy.Api.Media.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;
using Mealthy.Api.Shared.Infraestructure.Web;
using Mealthy.Api.Shared.Interfaces.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Mealthy.Api.Media.Interfaces.Rest;

[ApiController]
[Route("api/v1/images")]
[Authorize]
[Produces("application/json")]
public class ImagesController(ImageService images) : ControllerBase
{
    /// <summary>Paso 1: reserva la imagen y devuelve la URL SAS para subirla directo a Blob Storage.</summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitingExtensions.UploadPolicy)]
    [ProducesResponseType<ImageUploadResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ImageUploadResponse>> CreateUpload(CreateImageUploadRequest req, CancellationToken ct)
    {
        var upload = await images.CreateUploadAsync(User.GetUserId(), req.ToCommand(), ct);
        return CreatedAtAction(nameof(GetById), new { id = upload.Image.Id }, upload.ToResponse());
    }

    /// <summary>Paso 3: el archivo ya está en el storage → se encola para generar las variantes.</summary>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType<ImageResponse>(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageResponse>> Complete(int id, CancellationToken ct)
    {
        var image = await images.CompleteUploadAsync(id, ct);
        return AcceptedAtAction(nameof(GetById), new { id }, image.ToResponse());
    }

    /// <summary>Estado de la imagen (el frontend lo consulta hasta que sea Ready).</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType<ImageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImageResponse>> GetById(int id, CancellationToken ct)
        => Ok((await images.GetAsync(id, ct)).ToResponse());

    /// <summary>
    /// URL estable para &lt;img src&gt;: redirige (302) al blob WebP de la variante pedida.
    /// El contrato del frontend no cambia si mañana el storage pasa detrás de un CDN.
    /// </summary>
    [HttpGet("{id:int}/content")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContent(int id, [FromQuery] string? size, CancellationToken ct)
    {
        var variant = size is null
            ? ImageVariant.Medium
            : ImageVariant.FromName(size)
              ?? throw new BusinessRuleException($"size debe ser: {string.Join(", ", ImageVariant.All.Select(v => v.Name))}.");

        var url = await images.GetVariantUriAsync(id, variant, ct);

        // Una imagen Ready nunca cambia (reemplazarla = otra imagen con otro id) → el redirect se puede cachear
        Response.Headers.CacheControl = "public, max-age=86400";
        return Redirect(url.ToString());
    }
}
