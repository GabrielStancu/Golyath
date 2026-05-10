using Golyath.Application.DTOs;
using Golyath.Core.Enums;

namespace Golyath.Application.Services;

public interface IGoalService
{
    /// <summary>
    /// Returns all goals for the user, with progress recalculated from workout data.
    /// </summary>
    Task<IReadOnlyList<GoalSummary>> GetGoalsAsync(int userId);

    Task CreateGoalAsync(
        int userId,
        GoalType type,
        string description,
        double targetValue,
        int? exerciseId,
        DateTime? targetDate);

    Task DeleteGoalAsync(int goalId);

    /// <summary>Manually marks a goal as completed regardless of current progress.</summary>
    Task CompleteGoalAsync(int goalId);

    /// <summary>Returns all exercises for the goal exercise picker.</summary>
    Task<IReadOnlyList<ExerciseOption>> GetAllExercisesAsync();
}
