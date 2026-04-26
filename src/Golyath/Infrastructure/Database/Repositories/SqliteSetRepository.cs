using SQLite;
using Golyath.Application.Interfaces;
using Golyath.Core.Entities;

namespace Golyath.Infrastructure.Database.Repositories;

public sealed class SqliteSetRepository : SqliteRepository<Set>, ISetRepository
{
    public SqliteSetRepository(DatabaseContext context) : base(context) { }

    public SqliteSetRepository(SQLiteAsyncConnection connection) : base(connection) { }

    public async Task<IEnumerable<Set>> GetByWorkoutExerciseAsync(int workoutExerciseId)
    {
        var db = await GetConnectionAsync();
        return await db.Table<Set>()
            .Where(s => s.WorkoutExerciseId == workoutExerciseId)
            .ToListAsync();
    }
}
