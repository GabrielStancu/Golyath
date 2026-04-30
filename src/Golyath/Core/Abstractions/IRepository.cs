using Golyath.Core.Entities;

namespace Golyath.Core.Abstractions;

/// <summary>
/// Generic async repository contract. All data access is non-blocking.
/// </summary>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(int id);
    Task<IReadOnlyList<TEntity>> GetAllAsync();
    Task<int> InsertAsync(TEntity entity);
    Task<int> UpdateAsync(TEntity entity);
    Task<int> DeleteAsync(TEntity entity);
    Task<int> DeleteByIdAsync(int id);
}
