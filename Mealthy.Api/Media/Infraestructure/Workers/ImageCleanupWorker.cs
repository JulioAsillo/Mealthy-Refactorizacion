using Mealthy.Api.Media.Application;
using Mealthy.Api.Media.Application.Services;
using Microsoft.Extensions.Options;

namespace Mealthy.Api.Media.Infraestructure.Workers;

/// <summary>
/// Cada <see cref="MediaOptions.CleanupInterval"/> borra imágenes huérfanas (blobs + fila).
/// Así reemplazar o quitar una foto no deja basura en el storage, sin acoplar Media a los demás contextos.
/// </summary>
public sealed class ImageCleanupWorker(
    IServiceScopeFactory scopes,
    IOptions<MediaOptions> options,
    ILogger<ImageCleanupWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.CleanupInterval);

        do
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var deleted = await scope.ServiceProvider.GetRequiredService<ImageService>()
                    .CleanupOrphansAsync(stoppingToken);

                if (deleted > 0)
                    logger.LogInformation("Limpieza de imágenes: {Count} huérfana(s) eliminada(s).", deleted);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Falló la limpieza de imágenes huérfanas.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
