using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Core.Enums;
using Golyath.Infrastructure.Database;

namespace Golyath.Infrastructure.Repositories;

public sealed class ExerciseRepository : BaseRepository<Exercise>, IExerciseRepository
{
    public ExerciseRepository(DatabaseService databaseService) : base(databaseService) { }

    public async Task<IReadOnlyList<Exercise>> SearchAsync(string query)
    {
        var db = await GetDbAsync();
        var normalized = query.Trim().ToLowerInvariant();
        return await db.Table<Exercise>()
            .Where(e => e.Name.ToLower().Contains(normalized))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Exercise>> GetByMuscleGroupAsync(MuscleGroup muscleGroup)
    {
        var db = await GetDbAsync();
        return await db.Table<Exercise>()
            .Where(e => e.PrimaryMuscle == muscleGroup)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Exercise>> GetByEquipmentAsync(EquipmentType equipment)
    {
        var db = await GetDbAsync();
        return await db.Table<Exercise>()
            .Where(e => e.Equipment == equipment)
            .ToListAsync();
    }
}
