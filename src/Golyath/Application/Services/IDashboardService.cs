using Golyath.Application.DTOs;

namespace Golyath.Application.Services;

public interface IDashboardService
{
    Task<LastWorkoutSummary?> GetLastWorkoutSummaryAsync();
    Task<IReadOnlyList<WeeklyActivityDay>> GetWeeklyActivityAsync();
    Task<ReadinessInfo> GetReadinessAsync();
    Task<WorkoutSuggestion> GetWorkoutSuggestionAsync();
}
