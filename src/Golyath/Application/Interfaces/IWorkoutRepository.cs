using Golyath.Core.Entities;

namespace Golyath.Application.Interfaces;

public interface IWorkoutRepository : IRepository<Workout>
{
    Task<IEnumerable<Workout>> GetByDateRangeAsync(DateTime from, DateTime to);
}
