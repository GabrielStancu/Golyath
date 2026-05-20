using Golyath.Core.Entities;

namespace Golyath.Core.Abstractions;

public interface IRoutineRepository : IRepository<Routine>
{
    Task<IReadOnlyList<Routine>> GetAllOrderedAsync();
}
