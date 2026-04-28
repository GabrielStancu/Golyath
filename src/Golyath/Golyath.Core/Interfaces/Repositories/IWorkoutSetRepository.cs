using Golyath.Core.Entities;

namespace Golyath.Core.Interfaces.Repositories;

public interface IWorkoutSetRepository
{
    Task<WorkoutSet?> GetByIdAsync(int id);
    Task<IEnumerable<WorkoutSet>> GetByWorkoutExerciseIdAsync(int workoutExerciseId);
    Task<IEnumerable<WorkoutSet>> GetPersonalRecordsAsync(string exerciseId);
    Task<int> AddAsync(WorkoutSet set);
    Task UpdateAsync(WorkoutSet set);
    Task DeleteAsync(int id);
}
