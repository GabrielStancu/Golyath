using SQLite;

namespace Golyath.Infrastructure.Database.Migrations;

/// <summary>Adds the Language column to the Users table.</summary>
internal sealed class Migration005_UserLanguage : IMigration
{
    public int Version => 5;

    public async Task ApplyAsync(SQLiteAsyncConnection db)
    {
        try { await db.ExecuteAsync("ALTER TABLE Users ADD COLUMN Language INTEGER NOT NULL DEFAULT 0"); }
        catch { /* column may already exist */ }
    }
}
