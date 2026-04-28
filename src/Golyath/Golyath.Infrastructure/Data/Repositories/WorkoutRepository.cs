using Golyath.Core.Entities;
using Golyath.Core.Interfaces.Repositories;
using Golyath.Infrastructure.Data;
using Golyath.Infrastructure.Data.Models;

namespace Golyath.Infrastructure.Data.Repositories;

internal class WorkoutRepository : IWorkoutRepository
{
    private readonly AppDatabase _database;

    public WorkoutRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<Workout?> GetByIdAsync(int id)
    {
        var model = await _database.Connection.FindAsync<WorkoutDbModel>(id);
        return model is null ? null : MapToEntity(model);
    }

    public async Task<IEnumerable<Workout>> GetAllAsync()
    {
        var models = await _database.Connection.Table<WorkoutDbModel>()
            .OrderByDescending(w => w.StartedAt)
            .ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task<IEnumerable<Workout>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        var fromStr = from.ToString("o");
        var toStr = to.ToString("o");
        var models = await _database.Connection.QueryAsync<WorkoutDbModel>(
            "SELECT * FROM Workouts WHERE StartedAt >= ? AND StartedAt <= ? ORDER BY StartedAt DESC",
            fromStr, toStr);
        return models.Select(MapToEntity);
    }

    public async Task<IEnumerable<Workout>> GetRecentAsync(int count)
    {
        var models = await _database.Connection.Table<WorkoutDbModel>()
            .OrderByDescending(w => w.StartedAt)
            .Take(count)
            .ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task<Workout?> GetLastWorkoutAsync()
    {
        var model = await _database.Connection.Table<WorkoutDbModel>()
            .OrderByDescending(w => w.StartedAt)
            .FirstOrDefaultAsync();
        return model is null ? null : MapToEntity(model);
    }

    public async Task<int> AddAsync(Workout workout)
    {
        var model = MapToModel(workout);
        await _database.Connection.InsertAsync(model);
        return model.Id;
    }

    public async Task UpdateAsync(Workout workout)
    {
        await _database.Connection.UpdateAsync(MapToModel(workout));
    }

    public async Task DeleteAsync(int id)
    {
        await _database.Connection.DeleteAsync<WorkoutDbModel>(id);
    }

    private static Workout MapToEntity(WorkoutDbModel m) => new()
    {
        Id = m.Id,
        UserId = m.UserId,
        Name = m.Name,
        StartedAt = DateTime.Parse(m.StartedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
        CompletedAt = m.CompletedAt is null ? null : DateTime.Parse(m.CompletedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
        Notes = m.Notes,
        DurationSeconds = m.DurationSeconds,
        CreatedAt = DateTime.Parse(m.CreatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
    };

    private static WorkoutDbModel MapToModel(Workout e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        Name = e.Name,
        StartedAt = e.StartedAt.ToString("o"),
        CompletedAt = e.CompletedAt?.ToString("o"),
        Notes = e.Notes,
        DurationSeconds = e.DurationSeconds,
        CreatedAt = e.CreatedAt.ToString("o"),
    };
}
