using SQLite;
using Golyath.Application.Interfaces;
using Golyath.Core.Entities;

namespace Golyath.Infrastructure.Database.Repositories;

public sealed class SqliteExerciseRepository : SqliteRepository<Exercise>, IExerciseRepository
{
    public SqliteExerciseRepository(DatabaseContext context) : base(context) { }

    public SqliteExerciseRepository(SQLiteAsyncConnection connection) : base(connection) { }

    public async Task<IEnumerable<Exercise>> SearchAsync(string query)
    {
        var db = await GetConnectionAsync();
        var lower = query.ToLowerInvariant();
        return await db.Table<Exercise>()
            .Where(e => e.Name.ToLower().Contains(lower))
            .ToListAsync();
    }

    public async Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(string muscle)
    {
        var db = await GetConnectionAsync();
        var lower = muscle.ToLowerInvariant();
        return await db.Table<Exercise>()
            .Where(e => e.PrimaryMuscle.ToLower() == lower)
            .ToListAsync();
    }
}
