using System.IO.Compression;
using Microsoft.AspNetCore.ResponseCompression;

namespace Mealthy.Api.Shared.Infraestructure.Web;

public static class PerformanceExtensions
{
    /// <summary>Connection string que inyecta Aspire (builder.AddRedis("cache")).</summary>
    public const string RedisConnectionName = "cache";

    private static readonly TimeSpan CatalogCacheDuration = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Tres niveles:
    ///  - Output cache: respuestas HTTP completas del catálogo público, invalidadas por etiqueta.
    ///  - HybridCache: datos de aplicación (L1 en memoria del proceso + L2 en Redis, compartido entre instancias).
    ///  - Compresión Brotli/Gzip de las respuestas JSON.
    /// </summary>
    public static IHostApplicationBuilder AddApiPerformance(this IHostApplicationBuilder builder)
    {
        // Redis como IDistributedCache (Aspire.StackExchange.Redis.DistributedCaching): HybridCache lo usa como L2
        builder.AddRedisDistributedCache(RedisConnectionName);
        builder.Services.AddHybridCache();

        // En memoria: suficiente con una instancia. Con varias instancias, el store pasa a Redis
        // (Aspire.StackExchange.Redis.OutputCaching) sin tocar controllers ni políticas.
        builder.Services.AddOutputCache(o =>
        {
            o.AddPolicy(CachePolicies.Recipes, new PublicCatalogCachePolicy(CacheTags.Recipes, CatalogCacheDuration));
            o.AddPolicy(CachePolicies.Stores, new PublicCatalogCachePolicy(CacheTags.Stores, CatalogCacheDuration));
            o.AddPolicy(CachePolicies.Products, new PublicCatalogCachePolicy(CacheTags.Products, CatalogCacheDuration));
        });

        // EnableForHttps queda en false (por defecto): en producción comprime el proxy/CDN delante de la API.
        builder.Services.AddResponseCompression(o =>
        {
            o.Providers.Add<BrotliCompressionProvider>();
            o.Providers.Add<GzipCompressionProvider>();
        });
        builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
        builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

        return builder;
    }
}
