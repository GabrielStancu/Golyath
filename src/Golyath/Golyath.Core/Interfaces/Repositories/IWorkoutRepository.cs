using Golyath.Core.Entities;

namespace Golyath.Core.Interfaces.Repositories;

public interface IWorkoutRepository
{
    Task<Workout?> GetByIdAsync(int id);
    Task<IEnumerable<Workout>> GetAllAsync();
    Task<IEnumerable<Workout>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Workout>> GetRecentAsync(int count);
    Task<Workout?> GetLastWorkoutAsync();
    Task<int> AddAsync(Workout workout);
    Task UpdateAsync(Workout workout);
    Task DeleteAsync(int id);
}
