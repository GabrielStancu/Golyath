using Golyath.Core.Entities;

namespace Golyath.Core.Abstractions;

public interface IWorkoutExerciseRepository : IRepository<WorkoutExercise>
{
    Task<IReadOnlyList<WorkoutExercise>> GetByWorkoutIdAsync(int workoutId);
    Task<IReadOnlyList<WorkoutExercise>> GetByWorkoutIdsAsync(IEnumerable<int> workoutIds);
}
