using Golyath.Core.Entities;

namespace Golyath.Core.Abstractions;

public interface IRoutineExerciseRepository : IRepository<RoutineExercise>
{
    Task<IReadOnlyList<RoutineExercise>> GetByRoutineIdAsync(int routineId);
    Task DeleteByRoutineIdAsync(int routineId);
}
