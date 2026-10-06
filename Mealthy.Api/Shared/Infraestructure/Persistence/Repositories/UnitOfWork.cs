using Mealthy.Api.Shared.Domain.Repositories;

namespace Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWorks
{
    public Task CompleteAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}