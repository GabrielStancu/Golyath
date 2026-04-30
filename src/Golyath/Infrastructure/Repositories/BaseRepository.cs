using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Infrastructure.Database;
using SQLite;

namespace Golyath.Infrastructure.Repositories;

/// <summary>
/// Generic async repository. Concrete repositories inherit this and add
/// entity-specific queries via <see cref="GetDbAsync"/>.
/// </summary>
public abstract class BaseRepository<TEntity> : IRepository<TEntity>
    where TEntity : BaseEntity, new()
{
    private readonly DatabaseService _databaseService;

    protected BaseRepository(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    protected Task<SQLiteAsyncConnection> GetDbAsync() =>
        _databaseService.GetConnectionAsync();

    public async Task<TEntity?> GetByIdAsync(int id)
    {
        var db = await GetDbAsync();
        return await db.FindAsync<TEntity>(id);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<TEntity>().ToListAsync();
    }

    public async Task<int> InsertAsync(TEntity entity)
    {
        var db = await GetDbAsync();
        return await db.InsertAsync(entity);
    }

    public async Task<int> UpdateAsync(TEntity entity)
    {
        var db = await GetDbAsync();
        return await db.UpdateAsync(entity);
    }

    public async Task<int> DeleteAsync(TEntity entity)
    {
        var db = await GetDbAsync();
        return await db.DeleteAsync(entity);
    }

    public async Task<int> DeleteByIdAsync(int id)
    {
        var db = await GetDbAsync();
        return await db.DeleteAsync<TEntity>(id);
    }
}
