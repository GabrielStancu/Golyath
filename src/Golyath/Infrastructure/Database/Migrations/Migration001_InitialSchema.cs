using Golyath.Core.Entities;
using SQLite;

namespace Golyath.Infrastructure.Database.Migrations;

/// <summary>Creates all core tables for the initial app schema.</summary>
internal sealed class Migration001_InitialSchema : IMigration
{
    public int Version => 1;

    public async Task ApplyAsync(SQLiteAsyncConnection db)
    {
        await db.CreateTableAsync<User>();
        await db.CreateTableAsync<Exercise>();
        await db.CreateTableAsync<Workout>();
        await db.CreateTableAsync<WorkoutExercise>();
        await db.CreateTableAsync<WorkoutSet>();
        await db.CreateTableAsync<Goal>();
        await db.CreateTableAsync<Tag>();
        await db.CreateTableAsync<WorkoutTag>();
    }
}
