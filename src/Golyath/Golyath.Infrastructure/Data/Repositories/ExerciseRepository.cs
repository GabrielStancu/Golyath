using System.Text.Json;
using Golyath.Core.Entities;
using Golyath.Core.Interfaces.Repositories;
using Golyath.Infrastructure.Data;
using Golyath.Infrastructure.Data.Models;

namespace Golyath.Infrastructure.Data.Repositories;

internal class ExerciseRepository : IExerciseRepository
{
    private readonly AppDatabase _database;

    public ExerciseRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<Exercise?> GetByIdAsync(string id)
    {
        var model = await _database.Connection.FindAsync<ExerciseDbModel>(id);
        return model is null ? null : MapToEntity(model);
    }

    public async Task<IEnumerable<Exercise>> GetAllAsync()
    {
        var models = await _database.Connection.Table<ExerciseDbModel>().ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task<IEnumerable<Exercise>> SearchAsync(string query)
    {
        var lowerQuery = query.ToLowerInvariant();
        var models = await _database.Connection
            .Table<ExerciseDbModel>()
            .Where(e => e.Name.ToLower().Contains(lowerQuery) ||
                        (e.Equipment != null && e.Equipment.ToLower().Contains(lowerQuery)))
            .ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(string muscleGroup)
    {
        var lower = muscleGroup.ToLowerInvariant();
        var models = await _database.Connection
            .Table<ExerciseDbModel>()
            .Where(e => e.PrimaryMusclesJson.ToLower().Contains(lower) ||
                        e.SecondaryMusclesJson.ToLower().Contains(lower))
            .ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task<IEnumerable<Exercise>> GetByEquipmentAsync(string equipment)
    {
        var lower = equipment.ToLowerInvariant();
        var models = await _database.Connection
            .Table<ExerciseDbModel>()
            .Where(e => e.Equipment != null && e.Equipment.ToLower() == lower)
            .ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task AddAsync(Exercise exercise)
    {
        await _database.Connection.InsertAsync(MapToModel(exercise));
    }

    public async Task UpdateAsync(Exercise exercise)
    {
        await _database.Connection.UpdateAsync(MapToModel(exercise));
    }

    public async Task DeleteAsync(string id)
    {
        await _database.Connection.DeleteAsync<ExerciseDbModel>(id);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var count = await _database.Connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Exercises WHERE Id = ?", id);
        return count > 0;
    }

    public async Task<int> GetCountAsync()
    {
        return await _database.Connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Exercises");
    }

    private static Exercise MapToEntity(ExerciseDbModel m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Force = m.Force,
        Level = m.Level,
        Mechanic = m.Mechanic,
        Equipment = m.Equipment,
        PrimaryMuscles = JsonSerializer.Deserialize<List<string>>(m.PrimaryMusclesJson) ?? [],
        SecondaryMuscles = JsonSerializer.Deserialize<List<string>>(m.SecondaryMusclesJson) ?? [],
        Instructions = JsonSerializer.Deserialize<List<string>>(m.InstructionsJson) ?? [],
        Category = m.Category,
        IsCustom = m.IsCustom,
        CreatedAt = DateTime.Parse(m.CreatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
    };

    private static ExerciseDbModel MapToModel(Exercise e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Force = e.Force,
        Level = e.Level,
        Mechanic = e.Mechanic,
        Equipment = e.Equipment,
        PrimaryMusclesJson = JsonSerializer.Serialize(e.PrimaryMuscles),
        SecondaryMusclesJson = JsonSerializer.Serialize(e.SecondaryMuscles),
        InstructionsJson = JsonSerializer.Serialize(e.Instructions),
        Category = e.Category,
        IsCustom = e.IsCustom,
        CreatedAt = e.CreatedAt.ToString("o"),
    };
}
