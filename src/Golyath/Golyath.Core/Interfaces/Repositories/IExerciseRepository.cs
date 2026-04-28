using Golyath.Core.Entities;

namespace Golyath.Core.Interfaces.Repositories;

public interface IExerciseRepository
{
    Task<Exercise?> GetByIdAsync(string id);
    Task<IEnumerable<Exercise>> GetAllAsync();
    Task<IEnumerable<Exercise>> SearchAsync(string query);
    Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(string muscleGroup);
    Task<IEnumerable<Exercise>> GetByEquipmentAsync(string equipment);
    Task AddAsync(Exercise exercise);
    Task UpdateAsync(Exercise exercise);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<int> GetCountAsync();
}
