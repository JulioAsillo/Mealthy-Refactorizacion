namespace Mealthy.Api.Shared.Domain.Model;

/// <summary>
/// Entidad que no se borra físicamente: repository.Remove() la marca con DeletedAt
/// (AppDbContext.SaveChangesAsync) y un filtro global la oculta de todas las consultas.
/// Queda el registro de qué se eliminó y cuándo (moderación del Admin incluida).
/// </summary>
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
}
