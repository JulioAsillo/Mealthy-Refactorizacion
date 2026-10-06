using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Shared.Domain.Repositories;

public interface IBaseRepository<T> where T : Entity
{
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);
    Task<T?> FindByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}