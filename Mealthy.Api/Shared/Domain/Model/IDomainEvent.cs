namespace Mealthy.Api.Shared.Domain.Model;

/// <summary>
/// Algo que YA ocurrió en el dominio (nombre en pasado: ImageUploaded, RecipeDeleted…).
/// Se serializa a JSON en la tabla outbox_messages dentro de la misma transacción que el cambio,
/// así que debe ser un record pequeño con datos primitivos (ids), no entidades completas.
/// </summary>
public interface IDomainEvent;
