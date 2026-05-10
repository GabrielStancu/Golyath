using Golyath.Core.Enums;

namespace Golyath.Application.DTOs;

public record GoalSummary(
    int Id,
    GoalType Type,
    string Description,
    double TargetValue,
    double CurrentValue,
    double ProgressPercent,
    double ProgressRatio,
    string ProgressText,
    string TypeLabel,
    string? ExerciseName,
    string ProgressHint,
    bool IsCompleted,
    DateTime? TargetDate);
