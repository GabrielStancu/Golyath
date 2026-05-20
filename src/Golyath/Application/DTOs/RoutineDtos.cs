using Golyath.Core.Enums;

namespace Golyath.Application.DTOs;

public record RoutineSummaryDto(
    int Id,
    string Name,
    RoutineCategory Category,
    int ExerciseCount,
    int TotalSets,
    int EstimatedDurationMinutes)
{
    public string CategoryDisplayName => Category.DisplayName();
    public string CategoryHexColor => Category.HexColor();
    public string DurationLabel => $"~{EstimatedDurationMinutes} min";
    public string ExerciseLabel => ExerciseCount == 1 ? "1 exercise" : $"{ExerciseCount} exercises";
}

public record RoutineDetailDto(
    int Id,
    string Name,
    RoutineCategory Category,
    IReadOnlyList<RoutineExerciseDto> Exercises);

public record RoutineExerciseDto(
    int Id,
    int ExerciseId,
    string ExerciseName,
    int Order,
    int TargetSets,
    int TargetReps,
    double? TargetWeight,
    int RestSeconds);
