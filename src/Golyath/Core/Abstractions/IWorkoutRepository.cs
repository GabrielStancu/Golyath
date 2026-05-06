using Golyath.Core.Entities;

namespace Golyath.Core.Abstractions;

public interface IWorkoutRepository : IRepository<Workout>
{
    Task<IReadOnlyList<Workout>> GetRecentAsync(int count = 10);
    Task<Workout?> GetActiveWorkoutAsync();
    Task<Workout?> GetLastCompletedAsync();
    Task<IReadOnlyList<Workout>> GetCompletedInRangeAsync(DateTime from, DateTime to);
}
