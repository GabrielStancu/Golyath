using SQLite;

namespace Golyath.Infrastructure.Database.Migrations;

/// <summary>
/// Runs pending schema migrations in version order.
/// New migrations must be registered in <see cref="DatabaseService"/>.
/// </summary>
internal sealed class DatabaseMigrator
{
    private readonly SQLiteAsyncConnection _db;
    private readonly IReadOnlyList<IMigration> _migrations;

    public DatabaseMigrator(SQLiteAsyncConnection db, IEnumerable<IMigration> migrations)
    {
        _db = db;
        _migrations = [.. migrations.OrderBy(m => m.Version)];
    }

    public async Task MigrateAsync()
    {
        await _db.CreateTableAsync<SchemaVersion>();

        var applied = (await _db.Table<SchemaVersion>().ToListAsync())
            .Select(v => v.Version)
            .ToHashSet();

        foreach (var migration in _migrations.Where(m => !applied.Contains(m.Version)))
        {
            await migration.ApplyAsync(_db);

            await _db.InsertAsync(new SchemaVersion
            {
                Version = migration.Version,
                AppliedAt = DateTime.UtcNow
            });
        }
    }
}
