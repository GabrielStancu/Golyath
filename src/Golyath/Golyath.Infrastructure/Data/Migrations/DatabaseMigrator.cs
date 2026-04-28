using SQLite;

namespace Golyath.Infrastructure.Data.Migrations;

public class DatabaseMigrator
{
    private readonly SQLiteAsyncConnection _db;
    private readonly IEnumerable<IMigration> _migrations;

    public DatabaseMigrator(AppDatabase database, IEnumerable<IMigration> migrations)
    {
        _db = database.Connection;
        _migrations = migrations;
    }

    public async Task MigrateAsync()
    {
        await _db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS _Migrations (
                Version INTEGER PRIMARY KEY,
                AppliedAt TEXT NOT NULL
            )");

        var applied = await _db.QueryAsync<MigrationRecord>(
            "SELECT Version FROM _Migrations ORDER BY Version");
        var appliedVersions = applied.Select(m => m.Version).ToHashSet();

        foreach (var migration in _migrations.OrderBy(m => m.Version))
        {
            if (appliedVersions.Contains(migration.Version))
                continue;

            await migration.UpAsync(_db);
            await _db.ExecuteAsync(
                "INSERT INTO _Migrations (Version, AppliedAt) VALUES (?, ?)",
                migration.Version,
                DateTime.UtcNow.ToString("o"));
            appliedVersions.Add(migration.Version);
        }
    }

    private class MigrationRecord
    {
        public int Version { get; set; }
    }
}
