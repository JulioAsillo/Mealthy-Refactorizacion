using System.Diagnostics;
using System.Text.Json;
using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Shared.Infraestructure.Messaging;

/// <summary>
/// Publica los eventos del outbox: lee pendientes, ejecuta sus handlers y los marca procesados.
///
/// - FOR UPDATE SKIP LOCKED: varias instancias de la API pueden correrlo a la vez sin procesar
///   el mismo mensaje dos veces dentro de una misma ronda.
/// - Un mensaje que falla se reintenta en la siguiente ronda hasta <see cref="MaxAttempts"/>;
///   después queda en la tabla con su LastError para revisarlo a mano.
/// - Los procesados se borran tras <see cref="Retention"/>.
/// </summary>
public sealed class OutboxProcessor(
    IServiceScopeFactory scopes,
    IEnumerable<DomainEventHandlerRegistration> registrations,
    TimeProvider clock,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private const int MaxAttempts = 10;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan Retention = TimeSpan.FromDays(7);
    private static readonly TimeSpan PurgeInterval = TimeSpan.FromHours(1);
    private static readonly ActivitySource Activity = new("Mealthy.Api.Outbox");

    private readonly ILookup<string, DomainEventHandlerRegistration> _handlers =
        registrations.ToLookup(r => r.EventType.FullName!);

    private DateTime _lastPurge = DateTime.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);
        do
        {
            try
            {
                // Mientras haya lotes llenos se sigue sin esperar al siguiente tick
                while (await ProcessBatchAsync(stoppingToken) == BatchSize) { }
                await PurgeProcessedAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error en el procesador del outbox.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task<int> ProcessBatchAsync(CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Con EnableRetryOnFailure (Aspire) toda transacción explícita va dentro de la estrategia de ejecución
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();   // si la estrategia reintenta, se empieza limpio
            await using var transaction = await db.Database.BeginTransactionAsync(ct);

            var messages = await db.Set<OutboxMessage>()
                .FromSql($"""
                    SELECT * FROM outbox_messages
                    WHERE processed_at IS NULL AND attempts < {MaxAttempts}
                    ORDER BY id
                    LIMIT {BatchSize}
                    FOR UPDATE SKIP LOCKED
                    """)
                .ToListAsync(ct);

            foreach (var message in messages)
                await DispatchAsync(scope.ServiceProvider, message, ct);

            await db.SaveChangesAsync(ct);   // estado del outbox + lo que hayan cambiado los handlers
            await transaction.CommitAsync(ct);
            return messages.Count;
        });
    }

    private async Task DispatchAsync(IServiceProvider services, OutboxMessage message, CancellationToken ct)
    {
        using var activity = Activity.StartActivity($"outbox.dispatch {message.Type}", ActivityKind.Consumer);
        activity?.SetTag("outbox.message_id", message.Id);

        var handlers = _handlers[message.Type].ToList();
        if (handlers.Count == 0)
        {
            // Evento sin suscriptores (todavía): se da por procesado para no reintentarlo eternamente
            logger.LogDebug("Evento {Type} sin handlers registrados.", message.Type);
            message.MarkProcessed(clock.GetUtcNow().UtcDateTime);
            return;
        }

        try
        {
            var domainEvent = (IDomainEvent)(JsonSerializer.Deserialize(message.Payload, handlers[0].EventType)
                                             ?? throw new InvalidOperationException("Payload vacío."));

            foreach (var handler in handlers)
                await handler.Invoke(services, domainEvent, ct);

            message.MarkProcessed(clock.GetUtcNow().UtcDateTime);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            message.RecordFailure(ex.Message);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            if (message.Attempts >= MaxAttempts)
                logger.LogError(ex, "Outbox {Id} ({Type}) descartado tras {Attempts} intentos.",
                    message.Id, message.Type, message.Attempts);
            else
                logger.LogWarning(ex, "Outbox {Id} ({Type}) falló (intento {Attempts}/{Max}).",
                    message.Id, message.Type, message.Attempts, MaxAttempts);
        }
    }

    private async Task PurgeProcessedAsync(CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        if (now - _lastPurge < PurgeInterval) return;
        _lastPurge = now;

        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cutoff = now - Retention;

        var deleted = await db.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt != null && m.ProcessedAt < cutoff)
            .ExecuteDeleteAsync(ct);

        if (deleted > 0)
            logger.LogInformation("Outbox: {Count} mensaje(s) procesado(s) purgado(s).", deleted);
    }
}
