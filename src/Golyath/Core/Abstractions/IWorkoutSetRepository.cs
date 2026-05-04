using Golyath.Core.Entities;

namespace Golyath.Core.Abstractions;

public interface IWorkoutSetRepository : IRepository<WorkoutSet>
{
    Task<IReadOnlyList<WorkoutSet>> GetByWorkoutExerciseIdAsync(int workoutExerciseId);
    Task<WorkoutSet?> GetLastCompletedSetForExerciseAsync(int exerciseId);
}
