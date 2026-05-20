using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Infrastructure.Database;

namespace Golyath.Infrastructure.Repositories;

public sealed class RoutineExerciseRepository : BaseRepository<RoutineExercise>, IRoutineExerciseRepository
{
    public RoutineExerciseRepository(DatabaseService databaseService) : base(databaseService) { }

    public async Task<IReadOnlyList<RoutineExercise>> GetByRoutineIdAsync(int routineId)
    {
        var db = await GetDbAsync();
        return await db.Table<RoutineExercise>()
            .Where(re => re.RoutineId == routineId)
            .OrderBy(re => re.Order)
            .ToListAsync();
    }

    public async Task DeleteByRoutineIdAsync(int routineId)
    {
        var db = await GetDbAsync();
        await db.ExecuteAsync("DELETE FROM RoutineExercises WHERE RoutineId = ?", routineId);
    }
}
