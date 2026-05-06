using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Infrastructure.Database;

namespace Golyath.Infrastructure.Repositories;

public sealed class WorkoutExerciseRepository : BaseRepository<WorkoutExercise>, IWorkoutExerciseRepository
{
    public WorkoutExerciseRepository(DatabaseService databaseService) : base(databaseService) { }

    public async Task<IReadOnlyList<WorkoutExercise>> GetByWorkoutIdAsync(int workoutId)
    {
        var db = await GetDbAsync();
        return await db.Table<WorkoutExercise>()
            .Where(we => we.WorkoutId == workoutId)
            .OrderBy(we => we.Order)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<WorkoutExercise>> GetByWorkoutIdsAsync(IEnumerable<int> workoutIds)
    {
        var ids = workoutIds.ToHashSet();
        if (ids.Count == 0) return [];
        var db = await GetDbAsync();
        return await db.Table<WorkoutExercise>()
            .Where(we => ids.Contains(we.WorkoutId))
            .ToListAsync();
    }
}
