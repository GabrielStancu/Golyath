using Golyath.Core.Entities;

namespace Golyath.Application.Interfaces;

public interface ISetRepository : IRepository<Set>
{
    Task<IEnumerable<Set>> GetByWorkoutExerciseAsync(int workoutExerciseId);
}
