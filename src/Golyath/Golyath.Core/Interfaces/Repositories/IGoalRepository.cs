using Golyath.Core.Entities;

namespace Golyath.Core.Interfaces.Repositories;

public interface IGoalRepository
{
    Task<Goal?> GetByIdAsync(int id);
    Task<IEnumerable<Goal>> GetAllAsync();
    Task<IEnumerable<Goal>> GetActiveGoalsAsync();
    Task<int> AddAsync(Goal goal);
    Task UpdateAsync(Goal goal);
    Task DeleteAsync(int id);
}
