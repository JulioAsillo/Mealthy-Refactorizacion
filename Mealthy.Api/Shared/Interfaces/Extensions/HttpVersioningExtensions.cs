using System.Globalization;
using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;
using Microsoft.Net.Http.Headers;

namespace Mealthy.Api.Shared.Interfaces.Extensions;

/// <summary>Traducción entre la versión de la entidad y las cabeceras HTTP ETag / If-Match.</summary>
public static class HttpVersioningExtensions
{
    /// <summary>ETag: "123". El output cache lo guarda y responde 304 a un If-None-Match igual.</summary>
    public static void SetETag(this HttpResponse response, IVersioned entity)
        => response.Headers.ETag = $"\"{entity.Version.ToString(CultureInfo.InvariantCulture)}\"";

    /// <summary>
    /// Versión del If-Match ("123" o W/"123"). Null si no viene o es "*". Formato inválido → 400.
    /// </summary>
    public static uint? GetIfMatchVersion(this HttpRequest request)
    {
        var raw = request.Headers.IfMatch.ToString();
        if (string.IsNullOrWhiteSpace(raw) || raw.Trim() == "*") return null;

        if (EntityTagHeaderValue.TryParse(raw.Split(',')[0].Trim(), out var tag)
            && uint.TryParse(tag.Tag.ToString().Trim('"'), NumberStyles.None, CultureInfo.InvariantCulture, out var version))
            return version;

        throw new BusinessRuleException("La cabecera If-Match no tiene un formato válido.");
    }
}
