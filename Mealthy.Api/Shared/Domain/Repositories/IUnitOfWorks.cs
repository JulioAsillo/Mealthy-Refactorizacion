namespace Mealthy.Api.Shared.Domain.Repositories;

public interface IUnitOfWorks
{
    Task CompleteAsync(CancellationToken ct = default);
}