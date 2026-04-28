using Golyath.Core.Entities;
using Golyath.Core.Interfaces.Repositories;
using Golyath.Infrastructure.Data;
using Golyath.Infrastructure.Data.Models;

namespace Golyath.Infrastructure.Data.Repositories;

internal class WorkoutExerciseRepository : IWorkoutExerciseRepository
{
    private readonly AppDatabase _database;

    public WorkoutExerciseRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<WorkoutExercise?> GetByIdAsync(int id)
    {
        var model = await _database.Connection.FindAsync<WorkoutExerciseDbModel>(id);
        return model is null ? null : MapToEntity(model);
    }

    public async Task<IEnumerable<WorkoutExercise>> GetByWorkoutIdAsync(int workoutId)
    {
        var models = await _database.Connection
            .Table<WorkoutExerciseDbModel>()
            .Where(we => we.WorkoutId == workoutId)
            .OrderBy(we => we.Order)
            .ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task<int> AddAsync(WorkoutExercise workoutExercise)
    {
        var model = MapToModel(workoutExercise);
        await _database.Connection.InsertAsync(model);
        return model.Id;
    }

    public async Task UpdateAsync(WorkoutExercise workoutExercise)
    {
        await _database.Connection.UpdateAsync(MapToModel(workoutExercise));
    }

    public async Task DeleteAsync(int id)
    {
        await _database.Connection.DeleteAsync<WorkoutExerciseDbModel>(id);
    }

    private static WorkoutExercise MapToEntity(WorkoutExerciseDbModel m) => new()
    {
        Id = m.Id,
        WorkoutId = m.WorkoutId,
        ExerciseId = m.ExerciseId,
        Order = m.Order,
        Notes = m.Notes,
    };

    private static WorkoutExerciseDbModel MapToModel(WorkoutExercise e) => new()
    {
        Id = e.Id,
        WorkoutId = e.WorkoutId,
        ExerciseId = e.ExerciseId,
        Order = e.Order,
        Notes = e.Notes,
    };
}
