using Golyath.Core.Entities;

namespace Golyath.Application.Interfaces;

public interface IExerciseRepository : IRepository<Exercise>
{
    Task<IEnumerable<Exercise>> SearchAsync(string query);
    Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(string muscle);
}
