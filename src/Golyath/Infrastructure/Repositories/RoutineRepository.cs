using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Infrastructure.Database;

namespace Golyath.Infrastructure.Repositories;

public sealed class RoutineRepository : BaseRepository<Routine>, IRoutineRepository
{
    public RoutineRepository(DatabaseService databaseService) : base(databaseService) { }

    public async Task<IReadOnlyList<Routine>> GetAllOrderedAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<Routine>().OrderBy(r => r.Order).ToListAsync();
    }
}
