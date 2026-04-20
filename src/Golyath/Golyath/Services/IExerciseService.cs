using Golyath.Models;

namespace Golyath.Services;

public interface IExerciseService
{
    Task<List<Exercise>> GetAllExercisesAsync();
    Task<List<Exercise>> SearchExercisesAsync(string query, int? muscleGroupId = null, int? equipmentId = null);
    Task<Exercise?> GetExerciseByIdAsync(int id);
    Task<List<MuscleGroup>> GetMuscleGroupsAsync();
    Task<List<Equipment>> GetEquipmentAsync();
    Task<List<ExerciseMuscleGroup>> GetMuscleLinksForExerciseAsync(int exerciseId);
    Task<int> CreateExerciseAsync(Exercise exercise);
    Task UpdateExerciseAsync(Exercise exercise);
}
