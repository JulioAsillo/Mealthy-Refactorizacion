using Mealthy.Api.Shared.Application.Events;
using Mealthy.Api.Shared.Domain.Model;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Mealthy.Api.Shared.Infraestructure.Messaging;

public static class OutboxExtensions
{
    /// <summary>Registra el procesador del outbox (un solo BackgroundService para todos los contextos).</summary>
    public static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddHostedService<OutboxProcessor>();
        return services;
    }

    /// <summary>
    /// Suscribe un handler a un evento. Cada módulo registra los suyos en su XxxModule.cs:
    ///   services.AddDomainEventHandler&lt;ImageUploaded, EnqueueProcessingWhenImageUploaded&gt;();
    /// </summary>
    public static IServiceCollection AddDomainEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : IDomainEvent
        where THandler : class, IDomainEventHandler<TEvent>
    {
        services.AddScoped<THandler>();
        services.AddSingleton(new DomainEventHandlerRegistration(
            typeof(TEvent),
            (sp, domainEvent, ct) => sp.GetRequiredService<THandler>().HandleAsync((TEvent)domainEvent, ct)));
        return services;
    }
}
