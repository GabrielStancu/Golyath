using SQLite;
using Golyath.Application.Interfaces;

namespace Golyath.Infrastructure.Database.Repositories;

public abstract class SqliteRepository<T> : IRepository<T> where T : class, new()
{
    protected readonly DatabaseContext? Context;
    private readonly SQLiteAsyncConnection? _directConnection;

    protected SqliteRepository(DatabaseContext context)
    {
        Context = context;
    }

    // Test-friendly constructor: accept a raw connection, bypassing DatabaseContext/FileSystem
    protected SqliteRepository(SQLiteAsyncConnection connection)
    {
        _directConnection = connection;
    }

    protected async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_directConnection is not null)
            return _directConnection;
        return await Context!.GetConnectionAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.FindAsync<T>(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<T>().ToListAsync();
    }

    public async Task<int> InsertAsync(T entity)
    {
        var db = await GetConnectionAsync();
        return await db.InsertAsync(entity);
    }

    public async Task<int> UpdateAsync(T entity)
    {
        var db = await GetConnectionAsync();
        return await db.UpdateAsync(entity);
    }

    public async Task<int> DeleteAsync(int id)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync<T>(id);
    }
}
