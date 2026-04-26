using SQLite;

namespace Golyath.Infrastructure.Database;

public sealed class DatabaseContext : IAsyncDisposable
{
    private SQLiteAsyncConnection? _connection;
    private readonly string _dbPath;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public DatabaseContext(string path)
    {
        _dbPath = path;
    }

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        await _initLock.WaitAsync();
        try
        {
            if (_connection is null)
            {
                _connection = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            }
            return _connection;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            _connection = null;
        }
    }
}
