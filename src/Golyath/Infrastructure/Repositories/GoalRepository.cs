using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Infrastructure.Database;

namespace Golyath.Infrastructure.Repositories;

public sealed class GoalRepository : BaseRepository<Goal>, IGoalRepository
{
    public GoalRepository(DatabaseService databaseService) : base(databaseService) { }

    public async Task<IReadOnlyList<Goal>> GetByUserIdAsync(int userId)
    {
        var db = await GetDbAsync();
        return await db.Table<Goal>()
            .Where(g => g.UserId == userId)
            .OrderBy(g => g.CreatedAt)
            .ToListAsync();
    }
}
