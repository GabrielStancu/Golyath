using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Infrastructure.Database;

namespace Golyath.Infrastructure.Repositories;

public sealed class WorkoutRepository : BaseRepository<Workout>, IWorkoutRepository
{
    public WorkoutRepository(DatabaseService databaseService) : base(databaseService) { }

    public async Task<IReadOnlyList<Workout>> GetRecentAsync(int count = 10)
    {
        var db = await GetDbAsync();
        return await db.Table<Workout>()
            .OrderByDescending(w => w.StartedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Workout?> GetActiveWorkoutAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<Workout>()
            .Where(w => w.CompletedAt == null)
            .OrderByDescending(w => w.StartedAt)
            .FirstOrDefaultAsync();
    }
}
