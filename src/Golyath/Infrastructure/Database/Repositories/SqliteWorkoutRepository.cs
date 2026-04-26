using SQLite;
using Golyath.Application.Interfaces;
using Golyath.Core.Entities;

namespace Golyath.Infrastructure.Database.Repositories;

public sealed class SqliteWorkoutRepository : SqliteRepository<Workout>, IWorkoutRepository
{
    public SqliteWorkoutRepository(DatabaseContext context) : base(context) { }

    public SqliteWorkoutRepository(SQLiteAsyncConnection connection) : base(connection) { }

    public async Task<IEnumerable<Workout>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var db = await GetConnectionAsync();
        return await db.Table<Workout>()
            .Where(w => w.StartedAt >= from && w.StartedAt <= to)
            .ToListAsync();
    }
}
