using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;

namespace Mealthy.Api.Shared.Infraestructure.Web;

/// <summary>
/// Política para lecturas PÚBLICAS del catálogo (recetas, tiendas, productos).
///
/// La política por defecto de ASP.NET no cachea peticiones autenticadas, y el frontend envía el
/// Bearer en todas las llamadas cuando hay sesión: el caché casi nunca se usaría. Estas respuestas
/// no dependen del usuario (los endpoints son [AllowAnonymous]), así que se cachean igual.
/// NUNCA aplicar esta política a endpoints cuyo contenido dependa del usuario (p. ej. /stores/me).
/// </summary>
public sealed class PublicCatalogCachePolicy(string tag, TimeSpan duration) : IOutputCachePolicy
{
    ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var method = context.HttpContext.Request.Method;
        var cacheable = HttpMethods.IsGet(method) || HttpMethods.IsHead(method);

        context.EnableOutputCaching = cacheable;
        context.AllowCacheLookup = cacheable;
        context.AllowCacheStorage = cacheable;
        context.AllowLocking = true;                      // 100 peticiones simultáneas → 1 sola consulta a la BD
        context.ResponseExpirationTimeSpan = duration;
        context.CacheVaryByRules.QueryKeys = "*";         // ?page=, ?search=, ?category=… son entradas distintas
        context.Tags.Add(tag);

        return ValueTask.CompletedTask;
    }

    ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
        => ValueTask.CompletedTask;

    ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var response = context.HttpContext.Response;

        // Solo 200: un 404 o un 500 no se guardan
        if (response.StatusCode != StatusCodes.Status200OK || !StringValues.IsNullOrEmpty(response.Headers.SetCookie))
            context.AllowCacheStorage = false;

        return ValueTask.CompletedTask;
    }
}
