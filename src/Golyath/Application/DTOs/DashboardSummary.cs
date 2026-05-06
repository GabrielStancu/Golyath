using Golyath.Core.Enums;

namespace Golyath.Application.DTOs;

/// <summary>Summary of the most recent completed workout.</summary>
public record LastWorkoutSummary(
    string? Name,
    DateTime CompletedAt,
    int ExerciseCount,
    int SetCount,
    double TotalVolumeKg,
    int DurationSeconds);

/// <summary>Represents one day in the current week's activity view.</summary>
public record WeeklyActivityDay(string Label, bool HasWorkout, bool IsToday);

public enum ReadinessLevel { Rest, Moderate, Ready }

/// <summary>Heuristic readiness state derived from rest days since last workout.</summary>
public record ReadinessInfo(
    ReadinessLevel Level,
    string Label,
    string Message,
    int DaysSinceLastWorkout);

/// <summary>A suggested muscle group focus for the next workout.</summary>
public record WorkoutSuggestion(MuscleGroup MuscleGroup, string Reason)
{
    public string MuscleGroupName => MuscleGroup.ToString();
}
