using SQLite;

namespace Golyath.Infrastructure.Database.Migrations;

/// <summary>
/// Adds ExternalId and InstructionsRaw columns to the Exercises table.
/// </summary>
internal sealed class Migration002_ExerciseMetadata : IMigration
{
    public int Version => 2;

    public async Task ApplyAsync(SQLiteAsyncConnection db)
    {
        // On a fresh install Migration001 already created these columns (sqlite-net reflects
        // the current entity), so guard before altering to avoid "duplicate column" errors.
        var existing = (await db.QueryAsync<ColumnInfo>("PRAGMA table_info(Exercises)"))
            .Select(c => c.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!existing.Contains("ExternalId"))
            await db.ExecuteAsync("ALTER TABLE Exercises ADD COLUMN ExternalId TEXT");

        if (!existing.Contains("InstructionsRaw"))
            await db.ExecuteAsync("ALTER TABLE Exercises ADD COLUMN InstructionsRaw TEXT NOT NULL DEFAULT ''");
    }

    private sealed class ColumnInfo
    {
        [Column("name")]
        public string Name { get; set; } = string.Empty;
    }
}
