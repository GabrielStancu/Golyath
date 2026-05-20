using Golyath.Core.Entities;
using SQLite;

namespace Golyath.Infrastructure.Database.Migrations;

/// <summary>Adds Routine and RoutineExercise tables, and RoutineId column to Workouts.</summary>
internal sealed class Migration003_Routines : IMigration
{
    public int Version => 3;

    public async Task ApplyAsync(SQLiteAsyncConnection db)
    {
        await db.CreateTableAsync<Routine>();
        await db.CreateTableAsync<RoutineExercise>();

        // Add RoutineId column to existing Workouts table
        try
        {
            await db.ExecuteAsync("ALTER TABLE Workouts ADD COLUMN RoutineId INTEGER");
        }
        catch (SQLiteException)
        {
            // Column may already exist if re-running migration
        }
    }
}
