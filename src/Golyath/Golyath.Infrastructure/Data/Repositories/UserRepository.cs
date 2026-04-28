using Golyath.Core.Entities;
using Golyath.Core.Interfaces.Repositories;
using Golyath.Core.Enums;
using Golyath.Infrastructure.Data;
using Golyath.Infrastructure.Data.Models;

namespace Golyath.Infrastructure.Data.Repositories;

internal class UserRepository : IUserRepository
{
    private readonly AppDatabase _database;

    public UserRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var model = await _database.Connection
            .Table<UserDbModel>()
            .FirstOrDefaultAsync();
        return model is null ? null : MapToEntity(model);
    }

    public async Task<int> AddAsync(User user)
    {
        var model = MapToModel(user);
        await _database.Connection.InsertAsync(model);
        return model.Id;
    }

    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _database.Connection.UpdateAsync(MapToModel(user));
    }

    private static User MapToEntity(UserDbModel m) => new()
    {
        Id = m.Id,
        Nickname = m.Nickname,
        Birthday = m.Birthday is null ? null : DateTime.Parse(m.Birthday, null, System.Globalization.DateTimeStyles.RoundtripKind),
        HeightCm = m.HeightCm,
        WeightKg = m.WeightKg,
        Gender = m.Gender is null ? null : (Gender)m.Gender,
        FitnessGoal = m.FitnessGoal is null ? null : (FitnessGoal)m.FitnessGoal,
        UnitSystem = (UnitSystem)m.UnitSystem,
        OnboardingCompleted = m.OnboardingCompleted,
        CreatedAt = DateTime.Parse(m.CreatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
        UpdatedAt = DateTime.Parse(m.UpdatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
    };

    private static UserDbModel MapToModel(User e) => new()
    {
        Id = e.Id,
        Nickname = e.Nickname,
        Birthday = e.Birthday?.ToString("o"),
        HeightCm = e.HeightCm,
        WeightKg = e.WeightKg,
        Gender = e.Gender.HasValue ? (int)e.Gender.Value : null,
        FitnessGoal = e.FitnessGoal.HasValue ? (int)e.FitnessGoal.Value : null,
        UnitSystem = (int)e.UnitSystem,
        OnboardingCompleted = e.OnboardingCompleted,
        CreatedAt = e.CreatedAt.ToString("o"),
        UpdatedAt = e.UpdatedAt.ToString("o"),
    };
}
