using System.Diagnostics;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Mealthy.Api.Media.Application;
using Mealthy.Api.Media.Application.Services;
using Microsoft.Extensions.Options;

namespace Mealthy.Api.Media.Infraestructure.Workers;

/// <summary>
/// Consumidor de la cola "image-processing". Corre dentro del proceso de la API (BackgroundService);
/// si un día pesa demasiado, se mueve tal cual a un proyecto Worker aparte sin tocar la capa de aplicación.
///
/// Semántica at-least-once de Storage Queues: un mensaje puede llegar dos veces → el procesamiento es idempotente.
/// Si falla, el mensaje reaparece al vencer el visibility timeout (reintento automático).
/// </summary>
public sealed class ImageProcessingWorker(
    QueueServiceClient queues,
    IServiceScopeFactory scopes,
    IOptions<MediaOptions> options,
    ILogger<ImageProcessingWorker> logger) : BackgroundService
{
    private const int BatchSize = 8;
    private const int MaxAttempts = 5;
    private static readonly TimeSpan VisibilityTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan MinIdleDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxIdleDelay = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queue = queues.GetQueueClient(options.Value.ProcessingQueue);
        var idleDelay = MinIdleDelay;

        while (!stoppingToken.IsCancellationRequested)
        {
            QueueMessage[] messages;
            try
            {
                messages = (await queue.ReceiveMessagesAsync(BatchSize, VisibilityTimeout, stoppingToken)).Value;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo leer la cola {Queue}. Reintentando…", queue.Name);
                await Task.Delay(MaxIdleDelay, stoppingToken);
                continue;
            }

            if (messages.Length == 0)
            {
                // Backoff en vacío: 1 s, 2 s, 4 s… hasta 10 s. Vuelve a 1 s al llegar trabajo.
                await Task.Delay(idleDelay, stoppingToken);
                idleDelay = TimeSpan.FromTicks(Math.Min(idleDelay.Ticks * 2, MaxIdleDelay.Ticks));
                continue;
            }

            idleDelay = MinIdleDelay;
            foreach (var message in messages)
                await HandleAsync(queue, message, stoppingToken);
        }
    }

    private async Task HandleAsync(QueueClient queue, QueueMessage message, CancellationToken ct)
    {
        if (!int.TryParse(message.Body.ToString(), out var imageId))
        {
            logger.LogError("Mensaje inválido descartado: {Body}", message.Body.ToString());
            await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);
            return;
        }

        using var activity = MediaTelemetry.Source.StartActivity("image.process", ActivityKind.Consumer);
        activity?.SetTag("image.id", imageId);
        activity?.SetTag("messaging.attempt", message.DequeueCount);

        try
        {
            await using var scope = scopes.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<ImageProcessingService>().ProcessAsync(imageId, ct);
            await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;   // apagado: el mensaje reaparecerá y lo tomará la siguiente ejecución
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            logger.LogError(ex, "Error procesando la imagen {ImageId} (intento {Attempt}/{Max}).",
                imageId, message.DequeueCount, MaxAttempts);

            if (message.DequeueCount >= MaxAttempts)
            {
                // "Poison message": se marca Failed y se descarta para no reintentar para siempre
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<ImageProcessingService>()
                    .FailAsync(imageId, "No se pudo procesar la imagen. Inténtalo de nuevo.", ct);
                await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);
            }
        }
    }
}
