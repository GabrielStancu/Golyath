using Golyath.Infrastructure.Database.Migrations;
using SQLite;

namespace Golyath.Infrastructure.Database;

/// <summary>
/// Manages the SQLite connection lifetime and ensures migrations run before first use.
/// Register as a singleton in DI.
/// </summary>
public sealed class DatabaseService : IAsyncDisposable
{
    private const string DatabaseFileName = "golyath.db";

    private SQLiteAsyncConnection? _connection;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;

    /// <summary>
    /// Returns the initialized connection. Safe to call from multiple threads —
    /// initialization runs exactly once.
    /// </summary>
    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_initialized && _connection is not null)
            return _connection;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized && _connection is not null)
                return _connection;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);

            _connection = new SQLiteAsyncConnection(
                dbPath,
                SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

            var migrator = new DatabaseMigrator(_connection, GetMigrations());
            await migrator.MigrateAsync();

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }

        return _connection;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.CloseAsync();

        _initLock.Dispose();
    }

    /// <summary>
    /// Add new migrations here in ascending version order.
    /// </summary>
    private static IEnumerable<IMigration> GetMigrations() =>
    [
        new Migration001_InitialSchema(),
        new Migration002_ExerciseMetadata(),
        new Migration003_Routines(),
        new Migration004_RoutineRestSeconds()
    ];
}
