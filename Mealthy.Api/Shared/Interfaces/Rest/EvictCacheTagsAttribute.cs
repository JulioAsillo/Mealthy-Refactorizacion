using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OutputCaching;

namespace Mealthy.Api.Shared.Interfaces.Rest;

/// <summary>
/// Tras una escritura exitosa (POST/PUT/PATCH/DELETE con status &lt; 400) invalida las etiquetas
/// del output cache. Se declara a nivel de controller: las lecturas lo ignoran.
/// Si la acción lanza excepción, el filtro de resultado no se ejecuta y no se invalida nada.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class EvictCacheTagsAttribute(params string[] tags) : Attribute, IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        var http = context.HttpContext;
        if (HttpMethods.IsGet(http.Request.Method) || HttpMethods.IsHead(http.Request.Method)) return;
        if (http.Response.StatusCode >= StatusCodes.Status400BadRequest) return;

        var store = http.RequestServices.GetRequiredService<IOutputCacheStore>();
        foreach (var tag in tags)
            await store.EvictByTagAsync(tag, CancellationToken.None);   // aunque el cliente corte, el caché debe quedar limpio
    }
}
