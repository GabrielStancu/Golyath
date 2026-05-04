using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Infrastructure.Database;

namespace Golyath.Infrastructure.Repositories;

public sealed class WorkoutSetRepository : BaseRepository<WorkoutSet>, IWorkoutSetRepository
{
    public WorkoutSetRepository(DatabaseService databaseService) : base(databaseService) { }

    public async Task<IReadOnlyList<WorkoutSet>> GetByWorkoutExerciseIdAsync(int workoutExerciseId)
    {
        var db = await GetDbAsync();
        return await db.Table<WorkoutSet>()
            .Where(s => s.WorkoutExerciseId == workoutExerciseId)
            .OrderBy(s => s.SetNumber)
            .ToListAsync();
    }

    public async Task<WorkoutSet?> GetLastCompletedSetForExerciseAsync(int exerciseId)
    {
        var db = await GetDbAsync();

        var workoutExercises = await db.Table<WorkoutExercise>()
            .Where(we => we.ExerciseId == exerciseId)
            .ToListAsync();

        if (workoutExercises.Count == 0)
            return null;

        var allSets = new List<WorkoutSet>();
        foreach (var we in workoutExercises)
        {
            var sets = await db.Table<WorkoutSet>()
                .Where(s => s.WorkoutExerciseId == we.Id && s.IsCompleted)
                .ToListAsync();
            allSets.AddRange(sets);
        }

        return allSets
            .Where(s => s.CompletedAt.HasValue)
            .OrderByDescending(s => s.CompletedAt)
            .FirstOrDefault();
    }
}
