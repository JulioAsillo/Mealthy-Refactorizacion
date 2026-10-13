using Mealthy.Api.Media.Application;
using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Media.Application.EventHandlers;
using Mealthy.Api.Media.Application.Services;
using Mealthy.Api.Media.Domain.Events;
using Mealthy.Api.Media.Domain.Repositories;
using Mealthy.Api.Media.Infraestructure.Persistence;
using Mealthy.Api.Media.Infraestructure.Processing;
using Mealthy.Api.Media.Infraestructure.Storage;
using Mealthy.Api.Media.Infraestructure.Workers;
using Mealthy.Api.Shared.Infraestructure.Messaging;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mealthy.Api.Media;

public static class MediaModule
{
    /// <summary>
    /// Recibe IHostApplicationBuilder (no IServiceCollection) porque las integraciones de Aspire
    /// leen las connection strings "blobs" y "queues" y registran health checks y trazas.
    /// </summary>
    public static IHostApplicationBuilder AddMediaModule(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOptions<MediaOptions>()
            .Bind(builder.Configuration.GetSection(MediaOptions.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Clientes de Azure Storage (Azurite en local) — Aspire.Azure.Storage.Blobs / .Queues
        builder.AddAzureBlobServiceClient(MediaOptions.BlobsConnectionName);
        builder.AddAzureQueueServiceClient(MediaOptions.QueuesConnectionName);

        builder.Services.TryAddSingleton(TimeProvider.System);   // reloj inyectable

        // Infraestructura
        builder.Services.AddScoped<IImageRepository, ImageRepository>();
        builder.Services.AddSingleton<IImageStorage, AzureBlobImageStorage>();
        builder.Services.AddSingleton<IImageProcessingQueue, AzureQueueImageProcessingQueue>();
        builder.Services.AddSingleton<IImageTranscoder, ImageSharpTranscoder>();

        // Aplicación (+ contrato público para Recipes y Marketplace)
        builder.Services.AddScoped<ImageService>();
        builder.Services.AddScoped<IImageAttachmentValidator>(sp => sp.GetRequiredService<ImageService>());
        builder.Services.AddScoped<ImageProcessingService>();

        // Eventos de dominio (vía outbox)
        builder.Services.AddDomainEventHandler<ImageUploaded, EnqueueProcessingWhenImageUploaded>();

        // Orden de arranque: primero crear contenedores/cola, luego los workers
        builder.Services.AddHostedService<MediaStorageInitializer>();
        builder.Services.AddHostedService<ImageProcessingWorker>();
        builder.Services.AddHostedService<ImageCleanupWorker>();

        return builder;
    }
}
