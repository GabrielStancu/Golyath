using Golyath.Data;
using Golyath.Models;

namespace Golyath.Services;

public class ExerciseService : IExerciseService
{
    private readonly GolyathDatabase _db;

    public ExerciseService(GolyathDatabase db)
    {
        _db = db;
    }

    public async Task<List<Exercise>> GetAllExercisesAsync()
    {
        return await _db.GetAllAsync<Exercise>();
    }

    public async Task<List<Exercise>> SearchExercisesAsync(
        string query, int? muscleGroupId = null, int? equipmentId = null)
    {
        var conn = await _db.GetRawConnectionAsync();

        var sql = new System.Text.StringBuilder(
            "SELECT * FROM Exercise WHERE 1=1");
        var args = new List<object>();

        if (!string.IsNullOrWhiteSpace(query))
        {
            sql.Append(" AND Name LIKE ?");
            args.Add($"%{query.Trim()}%");
        }

        if (muscleGroupId.HasValue)
        {
            sql.Append(" AND PrimaryMuscleGroupId = ?");
            args.Add(muscleGroupId.Value);
        }

        if (equipmentId.HasValue)
        {
            sql.Append(" AND EquipmentId = ?");
            args.Add(equipmentId.Value);
        }

        sql.Append(" ORDER BY Name ASC");

        return await conn.QueryAsync<Exercise>(sql.ToString(), args.ToArray());
    }

    public async Task<Exercise?> GetExerciseByIdAsync(int id)
    {
        return await _db.GetByIdAsync<Exercise>(id);
    }

    public async Task<List<MuscleGroup>> GetMuscleGroupsAsync()
    {
        return await _db.GetAllAsync<MuscleGroup>();
    }

    public async Task<List<Equipment>> GetEquipmentAsync()
    {
        return await _db.GetAllAsync<Equipment>();
    }

    public async Task<List<ExerciseMuscleGroup>> GetMuscleLinksForExerciseAsync(int exerciseId)
    {
        var conn = await _db.GetRawConnectionAsync();
        return await conn.QueryAsync<ExerciseMuscleGroup>(
            "SELECT * FROM ExerciseMuscleGroup WHERE ExerciseId = ?", exerciseId);
    }

    public async Task<int> CreateExerciseAsync(Exercise exercise)
    {
        exercise.IsCustom = true;
        return await _db.InsertAsync(exercise);
    }

    public async Task UpdateExerciseAsync(Exercise exercise)
    {
        await _db.UpdateAsync(exercise);
    }
}
