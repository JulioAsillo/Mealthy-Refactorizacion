using System.Security.Claims;
using System.Text.Json;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Mealthy.Api.Shared.Interfaces.Rest;

/// <summary>
/// Soporte de la cabecera "Idempotency-Key" en un POST: si el cliente reintenta con la misma clave
/// (doble clic, timeout de red, reintento automático) se devuelve la MISMA respuesta guardada en vez de
/// crear el recurso dos veces. La cabecera es opcional: sin ella el endpoint funciona igual que antes.
///
///  - Primera petición: se marca "en curso" (1 min) → se ejecuta → se guarda status + Location + body (24 h, Redis).
///  - Repetición: se responde lo guardado con "Idempotent-Replayed: true", sin tocar la BD.
///  - Repetición mientras la primera sigue en curso → 409.
///  - Si la acción falla (excepción o status ≥ 400) se borra la marca: el cliente puede reintentar con la misma clave.
/// La clave se aísla por usuario, método y ruta: dos usuarios no pueden colisionar.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentAttribute : Attribute, IAsyncActionFilter, IAsyncResultFilter
{
    public const string HeaderName = "Idempotency-Key";
    public const string ReplayedHeaderName = "Idempotent-Replayed";
    private const int MaxKeyLength = 100;
    private const string InFlightMarker = "__in_flight__";
    private const string CacheKeyItem = "Mealthy.Idempotency.CacheKey";

    private static readonly DistributedCacheEntryOptions InFlightOptions =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) };

    private static readonly DistributedCacheEntryOptions StoredOptions =
        new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };

    private sealed record StoredResponse(int StatusCode, string? Location, string Body);

    // ---------- Antes de la acción: ¿repetición? ----------

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        var key = http.Request.Headers[HeaderName].ToString().Trim();
        if (key.Length == 0)
        {
            await next();
            return;
        }

        if (key.Length > MaxKeyLength)
            throw new BusinessRuleException($"{HeaderName} no puede superar {MaxKeyLength} caracteres.");

        var cache = http.RequestServices.GetRequiredService<IDistributedCache>();
        var cacheKey = BuildCacheKey(http, key);
        var stored = await cache.GetStringAsync(cacheKey, http.RequestAborted);

        if (stored == InFlightMarker)
            throw new ConflictException("Ya hay una petición en curso con esta Idempotency-Key. Espera a que termine.");

        if (stored is not null)
        {
            Replay(context, JsonSerializer.Deserialize<StoredResponse>(stored)!);
            return;
        }

        await cache.SetStringAsync(cacheKey, InFlightMarker, InFlightOptions, http.RequestAborted);
        http.Items[CacheKeyItem] = cacheKey;

        var executed = await next();
        if (executed.Exception is not null && !executed.ExceptionHandled)
            await cache.RemoveAsync(cacheKey, CancellationToken.None);   // falló: se permite reintentar
    }

    // ---------- Después del resultado: guardar la respuesta ----------

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        var http = context.HttpContext;
        if (http.Items[CacheKeyItem] is not string cacheKey) return;   // sin clave, o fue una repetición

        var cache = http.RequestServices.GetRequiredService<IDistributedCache>();

        if (http.Response.StatusCode >= StatusCodes.Status400BadRequest
            || context.Result is not ObjectResult { Value: { } value })
        {
            await cache.RemoveAsync(cacheKey, CancellationToken.None);
            return;
        }

        // Mismas opciones JSON que usa MVC (camelCase, enums como texto) → la repetición es idéntica
        var json = http.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
        var stored = new StoredResponse(
            http.Response.StatusCode,
            http.Response.Headers.Location.ToString() is { Length: > 0 } location ? location : null,
            JsonSerializer.Serialize(value, value.GetType(), json));

        await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(stored), StoredOptions, CancellationToken.None);
    }

    // ---------- Privados ----------

    private static void Replay(ActionExecutingContext context, StoredResponse stored)
    {
        var headers = context.HttpContext.Response.Headers;
        headers[ReplayedHeaderName] = "true";
        if (stored.Location is not null) headers.Location = stored.Location;

        context.Result = new ContentResult
        {
            StatusCode = stored.StatusCode,
            Content = stored.Body,
            ContentType = "application/json; charset=utf-8"
        };
    }

    private static string BuildCacheKey(HttpContext http, string key)
    {
        var user = http.User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "anonymous";
        return $"idempotency:{user}:{http.Request.Method}:{http.Request.Path.Value?.ToLowerInvariant()}:{key}";
    }
}
