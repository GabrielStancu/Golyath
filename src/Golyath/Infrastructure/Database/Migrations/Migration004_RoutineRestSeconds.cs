using SQLite;

namespace Golyath.Infrastructure.Database.Migrations;

public sealed class Migration004_RoutineRestSeconds : IMigration
{
    public int Version => 4;

    public async Task ApplyAsync(SQLiteAsyncConnection db)
    {
        try { await db.ExecuteAsync("ALTER TABLE RoutineExercises ADD COLUMN RestSeconds INTEGER NOT NULL DEFAULT 90"); }
        catch { /* column may already exist */ }
    }
}
