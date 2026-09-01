namespace Golyath.Application.DTOs;

/// <summary>Summary row shown in the History list.</summary>
public record WorkoutHistorySummaryDto(
    int Id,
    string DisplayName,
    DateTime CompletedAt,
    int DurationSeconds,
    int ExerciseCount,
    int SetCount,
    double TotalVolumeKg,
    IReadOnlyList<string> TagNames,
    int PersonalRecordCount = 0)
{
    public string DurationFormatted =>
        DurationSeconds >= 3600
            ? $"{DurationSeconds / 3600}h {DurationSeconds % 3600 / 60}m"
            : $"{DurationSeconds / 60}m";

    public string VolumeFormatted => $"{TotalVolumeKg:0.#} kg";

    public string DateFormatted => CompletedAt.ToLocalTime().ToString("MMM d, yyyy");

    public string DayText => CompletedAt.ToLocalTime().Day.ToString();
    public string MonthText => CompletedAt.ToLocalTime().ToString("MMM").ToUpper();
    public string SubtitleText => $"{ExerciseCount} ex · {DurationFormatted} · {VolumeFormatted}";
    public bool HasPersonalRecords => PersonalRecordCount > 0;
    public string PrBadgeText => $"PR ×{PersonalRecordCount}";
}

/// <summary>Full breakdown shown on the Workout Detail page.</summary>
public record WorkoutHistoryDetailDto(
    int Id,
    string DisplayName,
    DateTime CompletedAt,
    int DurationSeconds,
    string? Notes,
    IReadOnlyList<WorkoutExerciseSummaryDto> Exercises,
    IReadOnlyList<string> TagNames,
    int PersonalRecordCount = 0)
{
    public string DurationFormatted =>
        DurationSeconds >= 3600
            ? $"{DurationSeconds / 3600}h {DurationSeconds % 3600 / 60}m"
            : $"{DurationSeconds / 60}m";

    public string DateFormatted => CompletedAt.ToLocalTime().ToString("dddd, MMMM d, yyyy");

    public bool HasPersonalRecords => PersonalRecordCount > 0;
    public string PrBadgeText => $"PR ×{PersonalRecordCount}";
}

/// <summary>One exercise entry within a workout detail.</summary>
public record WorkoutExerciseSummaryDto(
    int WorkoutExerciseId,
    string ExerciseName,
    string? ExerciseNotes,
    IReadOnlyList<SetSummaryDto> Sets)
{
    public string SetsSummary =>
        Sets.Count == 1 ? "1 set" : $"{Sets.Count} sets";
}

/// <summary>Single set row within an exercise in the detail view.</summary>
public record SetSummaryDto(
    int SetId,
    int SetNumber,
    double Weight,
    int Reps,
    string? Tempo,
    string? Notes)
{
    public string Label => $"Set {SetNumber}";
}
