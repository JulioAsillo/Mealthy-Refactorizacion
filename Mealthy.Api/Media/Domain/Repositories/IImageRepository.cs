using Mealthy.Api.Media.Domain.Model;
using Mealthy.Api.Shared.Domain.Repositories;

namespace Mealthy.Api.Media.Domain.Repositories;

public interface IImageRepository : IBaseRepository<Image>
{
    Task<Image?> GetReadOnlyAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Imágenes (con tracking) creadas antes de <paramref name="createdBefore"/> que ninguna
    /// entidad referencia. Incluye las fallidas y las que nunca se llegaron a subir.
    /// </summary>
    Task<IReadOnlyList<Image>> ListOrphansAsync(DateTime createdBefore, int take, CancellationToken ct = default);
}
