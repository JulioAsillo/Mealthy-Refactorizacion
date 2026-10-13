using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Mealthy.Api.Media.Application;
using Microsoft.Extensions.Options;

namespace Mealthy.Api.Media.Infraestructure.Storage;

/// <summary>
/// Al arrancar: crea contenedores y cola si no existen (idempotente). En Development además
/// configura CORS del Blob Service para que el navegador pueda hacer PUT directo a Azurite.
/// En Azure real esto se declara en la infraestructura (Bicep/Terraform), no en el código.
/// </summary>
public sealed class MediaStorageInitializer(
    BlobServiceClient blobs,
    QueueServiceClient queues,
    IOptions<MediaOptions> options,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<MediaStorageInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        var o = options.Value;

        await blobs.GetBlobContainerClient(o.UploadsContainer)
            .CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);
        await blobs.GetBlobContainerClient(o.ImagesContainer)
            .CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);   // lectura anónima por blob, sin listar
        await queues.GetQueueClient(o.ProcessingQueue)
            .CreateIfNotExistsAsync(cancellationToken: ct);

        if (environment.IsDevelopment())
            await ConfigureBrowserCorsAsync(ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

    private async Task ConfigureBrowserCorsAsync(CancellationToken ct)
    {
        // Mismos orígenes que la política CORS de la API (Aspire los inyecta en Cors__AllowedOrigins__0)
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length == 0) return;

        var properties = (await blobs.GetPropertiesAsync(ct)).Value;
        properties.Cors =
        [
            new BlobCorsRule
            {
                AllowedOrigins = string.Join(',', origins),
                AllowedMethods = "PUT,GET,HEAD,OPTIONS",
                AllowedHeaders = "*",
                ExposedHeaders = "*",
                MaxAgeInSeconds = 3600
            }
        ];
        await blobs.SetPropertiesAsync(properties, ct);

        logger.LogInformation("CORS de Blob Storage configurado para {Origins}", string.Join(", ", origins));
    }
}
