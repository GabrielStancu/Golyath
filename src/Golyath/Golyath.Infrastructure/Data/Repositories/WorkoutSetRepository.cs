using Golyath.Core.Entities;
using Golyath.Core.Interfaces.Repositories;
using Golyath.Infrastructure.Data;
using Golyath.Infrastructure.Data.Models;

namespace Golyath.Infrastructure.Data.Repositories;

internal class WorkoutSetRepository : IWorkoutSetRepository
{
    private readonly AppDatabase _database;

    public WorkoutSetRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<WorkoutSet?> GetByIdAsync(int id)
    {
        var model = await _database.Connection.FindAsync<WorkoutSetDbModel>(id);
        return model is null ? null : MapToEntity(model);
    }

    public async Task<IEnumerable<WorkoutSet>> GetByWorkoutExerciseIdAsync(int workoutExerciseId)
    {
        var models = await _database.Connection
            .Table<WorkoutSetDbModel>()
            .Where(s => s.WorkoutExerciseId == workoutExerciseId)
            .OrderBy(s => s.SetNumber)
            .ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task<IEnumerable<WorkoutSet>> GetPersonalRecordsAsync(string exerciseId)
    {
        var models = await _database.Connection.QueryAsync<WorkoutSetDbModel>(@"
            SELECT ws.* FROM WorkoutSets ws
            INNER JOIN WorkoutExercises we ON ws.WorkoutExerciseId = we.Id
            WHERE we.ExerciseId = ? AND ws.IsPersonalRecord = 1
            ORDER BY ws.WeightKg DESC", exerciseId);
        return models.Select(MapToEntity);
    }

    public async Task<int> AddAsync(WorkoutSet set)
    {
        var model = MapToModel(set);
        await _database.Connection.InsertAsync(model);
        return model.Id;
    }

    public async Task UpdateAsync(WorkoutSet set)
    {
        await _database.Connection.UpdateAsync(MapToModel(set));
    }

    public async Task DeleteAsync(int id)
    {
        await _database.Connection.DeleteAsync<WorkoutSetDbModel>(id);
    }

    private static WorkoutSet MapToEntity(WorkoutSetDbModel m) => new()
    {
        Id = m.Id,
        WorkoutExerciseId = m.WorkoutExerciseId,
        SetNumber = m.SetNumber,
        WeightKg = m.WeightKg,
        Reps = m.Reps,
        Tempo = m.Tempo,
        Notes = m.Notes,
        IsPersonalRecord = m.IsPersonalRecord,
        CompletedAt = m.CompletedAt is null ? null : DateTime.Parse(m.CompletedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
        CreatedAt = DateTime.Parse(m.CreatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
    };

    private static WorkoutSetDbModel MapToModel(WorkoutSet e) => new()
    {
        Id = e.Id,
        WorkoutExerciseId = e.WorkoutExerciseId,
        SetNumber = e.SetNumber,
        WeightKg = e.WeightKg,
        Reps = e.Reps,
        Tempo = e.Tempo,
        Notes = e.Notes,
        IsPersonalRecord = e.IsPersonalRecord,
        CompletedAt = e.CompletedAt?.ToString("o"),
        CreatedAt = e.CreatedAt.ToString("o"),
    };
}
