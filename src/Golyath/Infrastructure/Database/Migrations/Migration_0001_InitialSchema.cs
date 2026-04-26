using SQLite;
using Golyath.Application.Interfaces;
using Golyath.Core.Entities;

namespace Golyath.Infrastructure.Database.Migrations;

public sealed class Migration_0001_InitialSchema : IMigration
{
    public int Version => 1;

    public async Task UpAsync(SQLiteAsyncConnection db)
    {
        await db.CreateTableAsync<AppMetadata>();
        await db.CreateTableAsync<User>();
        await db.CreateTableAsync<Exercise>();
        await db.CreateTableAsync<Workout>();
        await db.CreateTableAsync<WorkoutExercise>();
        await db.CreateTableAsync<Set>();
        await db.CreateTableAsync<Goal>();
        await db.CreateTableAsync<Tag>();
    }
}
