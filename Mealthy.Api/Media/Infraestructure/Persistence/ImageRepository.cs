using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Media.Domain.Model;
using Mealthy.Api.Media.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Mealthy.Api.Media.Infraestructure.Persistence;

public class ImageRepository(AppDbContext context, IEnumerable<IImageReferenceSource> references)
    : BaseRepository<Image>(context), IImageRepository
{
    public Task<Image?> GetReadOnlyAsync(int id, CancellationToken ct = default)
        => Set.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IReadOnlyList<Image>> ListOrphansAsync(DateTime createdBefore, int take, CancellationToken ct = default)
    {
        // UNION de los ids que usa cada contexto → un solo "NOT IN (SELECT … UNION SELECT …)" en SQL
        var referenced = references
            .Select(r => r.ReferencedImageIds())
            .Aggregate((IQueryable<int>?)null, (acc, q) => acc is null ? q : acc.Union(q));

        var query = Set.Where(i => i.CreatedAt < createdBefore);
        if (referenced is { } inUse)
            query = query.Where(i => !inUse.Contains(i.Id));

        return await query
            .OrderBy(i => i.Id)
            .Take(take)
            .ToListAsync(ct);
    }
}
