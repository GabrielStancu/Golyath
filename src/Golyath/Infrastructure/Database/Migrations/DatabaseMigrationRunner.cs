using Golyath.Application.Interfaces;
using Golyath.Core.Entities;

namespace Golyath.Infrastructure.Database.Migrations;

public sealed class DatabaseMigrationRunner : IDatabaseMigrationRunner
{
    private const string SchemaVersionKey = "schema_version";
    private readonly DatabaseContext _context;
    private readonly IReadOnlyList<IMigration> _migrations;

    public DatabaseMigrationRunner(DatabaseContext context)
    {
        _context = context;
        _migrations = new List<IMigration>
        {
            new Migration_0001_InitialSchema()
        }.OrderBy(m => m.Version).ToList();
    }

    public async Task MigrateAsync()
    {
        var db = await _context.GetConnectionAsync();

        // Ensure AppMetadata table exists before querying it
        await db.CreateTableAsync<AppMetadata>();

        var currentVersion = await GetCurrentVersionAsync(db);

        foreach (var migration in _migrations.Where(m => m.Version > currentVersion))
        {
            await migration.UpAsync(db);
            await SetVersionAsync(db, migration.Version);
        }
    }

    private static async Task<int> GetCurrentVersionAsync(SQLite.SQLiteAsyncConnection db)
    {
        var record = await db.Table<AppMetadata>()
            .Where(m => m.Key == SchemaVersionKey)
            .FirstOrDefaultAsync();

        return record is null ? 0 : int.TryParse(record.Value, out var v) ? v : 0;
    }

    private static async Task SetVersionAsync(SQLite.SQLiteAsyncConnection db, int version)
    {
        var existing = await db.Table<AppMetadata>()
            .Where(m => m.Key == SchemaVersionKey)
            .FirstOrDefaultAsync();

        if (existing is null)
        {
            await db.InsertAsync(new AppMetadata { Key = SchemaVersionKey, Value = version.ToString() });
        }
        else
        {
            existing.Value = version.ToString();
            await db.UpdateAsync(existing);
        }
    }
}
