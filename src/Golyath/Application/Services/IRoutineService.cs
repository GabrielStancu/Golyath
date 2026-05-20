using Golyath.Application.DTOs;
using Golyath.Core.Entities;
using Golyath.Core.Enums;

namespace Golyath.Application.Services;

public interface IRoutineService
{
    Task<IReadOnlyList<RoutineSummaryDto>> GetAllRoutinesAsync();
    Task<IReadOnlyList<RoutineSummaryDto>> GetTopRoutinesAsync(int count);
    Task<RoutineDetailDto?> GetRoutineDetailAsync(int routineId);
    Task<Routine> CreateRoutineAsync(string name, RoutineCategory category);
    Task UpdateRoutineAsync(int routineId, string name, RoutineCategory category);
    Task DeleteRoutineAsync(int routineId);
    Task SetRoutineExercisesAsync(int routineId, IReadOnlyList<RoutineExerciseInput> exercises);
    Task<RoutineSummaryDto?> GetNextRoutineInRotationAsync();
    Task<int> GetEstimatedDurationAsync(int routineId);
}

public record RoutineExerciseInput(int ExerciseId, int Order, int TargetSets, int TargetReps, double? TargetWeight, int RestSeconds = 90);
