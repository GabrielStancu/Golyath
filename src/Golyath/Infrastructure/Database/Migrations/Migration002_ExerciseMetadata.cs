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
        // SQLite ALTER TABLE only supports ADD COLUMN — safe to run on existing data.
        await db.ExecuteAsync("ALTER TABLE Exercises ADD COLUMN ExternalId TEXT");
        await db.ExecuteAsync("ALTER TABLE Exercises ADD COLUMN InstructionsRaw TEXT NOT NULL DEFAULT ''");
    }
}
