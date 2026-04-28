using Golyath.Core.Entities;
using Golyath.Core.Enums;
using Golyath.Core.Interfaces.Repositories;
using Golyath.Infrastructure.Data;
using Golyath.Infrastructure.Data.Models;

namespace Golyath.Infrastructure.Data.Repositories;

internal class GoalRepository : IGoalRepository
{
    private readonly AppDatabase _database;

    public GoalRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<Goal?> GetByIdAsync(int id)
    {
        var model = await _database.Connection.FindAsync<GoalDbModel>(id);
        return model is null ? null : MapToEntity(model);
    }

    public async Task<IEnumerable<Goal>> GetAllAsync()
    {
        var models = await _database.Connection.Table<GoalDbModel>()
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task<IEnumerable<Goal>> GetActiveGoalsAsync()
    {
        var models = await _database.Connection.Table<GoalDbModel>()
            .Where(g => g.IsActive)
            .ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task<int> AddAsync(Goal goal)
    {
        var model = MapToModel(goal);
        await _database.Connection.InsertAsync(model);
        return model.Id;
    }

    public async Task UpdateAsync(Goal goal)
    {
        await _database.Connection.UpdateAsync(MapToModel(goal));
    }

    public async Task DeleteAsync(int id)
    {
        await _database.Connection.DeleteAsync<GoalDbModel>(id);
    }

    private static Goal MapToEntity(GoalDbModel m) => new()
    {
        Id = m.Id,
        UserId = m.UserId,
        Type = (GoalType)m.Type,
        ExerciseId = m.ExerciseId,
        TargetValue = m.TargetValue,
        StartDate = DateTime.Parse(m.StartDate, null, System.Globalization.DateTimeStyles.RoundtripKind),
        TargetDate = m.TargetDate is null ? null : DateTime.Parse(m.TargetDate, null, System.Globalization.DateTimeStyles.RoundtripKind),
        AchievedDate = m.AchievedDate is null ? null : DateTime.Parse(m.AchievedDate, null, System.Globalization.DateTimeStyles.RoundtripKind),
        IsActive = m.IsActive,
        CreatedAt = DateTime.Parse(m.CreatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
    };

    private static GoalDbModel MapToModel(Goal e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        Type = (int)e.Type,
        ExerciseId = e.ExerciseId,
        TargetValue = e.TargetValue,
        StartDate = e.StartDate.ToString("o"),
        TargetDate = e.TargetDate?.ToString("o"),
        AchievedDate = e.AchievedDate?.ToString("o"),
        IsActive = e.IsActive,
        CreatedAt = e.CreatedAt.ToString("o"),
    };
}
