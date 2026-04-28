using Golyath.Core.Entities;

namespace Golyath.Core.Interfaces.Repositories;

public interface IWorkoutExerciseRepository
{
    Task<WorkoutExercise?> GetByIdAsync(int id);
    Task<IEnumerable<WorkoutExercise>> GetByWorkoutIdAsync(int workoutId);
    Task<int> AddAsync(WorkoutExercise workoutExercise);
    Task UpdateAsync(WorkoutExercise workoutExercise);
    Task DeleteAsync(int id);
}
