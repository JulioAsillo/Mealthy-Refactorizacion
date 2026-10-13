namespace Mealthy.Api.Media.Application.Contracts;

/// <summary>
/// Contrato PÚBLICO que implementa cada bounded context que guarda un ImageId (Recipes, Marketplace…).
/// Media lo usa para saber qué imágenes siguen en uso sin conocer las tablas de los demás contextos.
/// Agregar un contexto nuevo con imágenes = registrar una implementación más (Open/Closed).
/// </summary>
public interface IImageReferenceSource
{
    /// <summary>Consulta (no ejecutada) con los ids de imagen referenciados. EF la traduce a un NOT IN.</summary>
    IQueryable<int> ReferencedImageIds();
}
