using Golyath.Core.Entities;

namespace Golyath.Core.Abstractions;

public interface IGoalRepository : IRepository<Goal>
{
    Task<IReadOnlyList<Goal>> GetByUserIdAsync(int userId);
}
