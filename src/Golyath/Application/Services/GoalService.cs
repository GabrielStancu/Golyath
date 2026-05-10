using Golyath.Application.DTOs;
using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Core.Enums;

namespace Golyath.Application.Services;

public sealed class GoalService : IGoalService
{
    private readonly IGoalRepository _goals;
    private readonly IWorkoutRepository _workouts;
    private readonly IWorkoutExerciseRepository _workoutExercises;
    private readonly IWorkoutSetRepository _workoutSets;
    private readonly IExerciseRepository _exercises;

    public GoalService(
        IGoalRepository goals,
        IWorkoutRepository workouts,
        IWorkoutExerciseRepository workoutExercises,
        IWorkoutSetRepository workoutSets,
        IExerciseRepository exercises)
    {
        _goals = goals;
        _workouts = workouts;
        _workoutExercises = workoutExercises;
        _workoutSets = workoutSets;
        _exercises = exercises;
    }

    public async Task<IReadOnlyList<GoalSummary>> GetGoalsAsync(int userId)
    {
        var goals = await _goals.GetByUserIdAsync(userId);
        if (goals.Count == 0) return [];

        // Recalculate progress for all goals before building summaries
        await RecalculateProgressAsync(goals);

        var summaries = new List<GoalSummary>(goals.Count);
        foreach (var goal in goals)
        {
            string? exerciseName = null;
            if (goal.ExerciseId.HasValue)
            {
                var ex = await _exercises.GetByIdAsync(goal.ExerciseId.Value);
                exerciseName = ex?.Name;
            }

            summaries.Add(BuildSummary(goal, exerciseName));
        }

        return summaries;
    }

    public async Task CreateGoalAsync(
        int userId,
        GoalType type,
        string description,
        double targetValue,
        int? exerciseId,
        DateTime? targetDate)
    {
        var goal = new Goal
        {
            UserId = userId,
            Type = type,
            Description = description,
            TargetValue = targetValue,
            CurrentValue = 0,
            ExerciseId = exerciseId,
            TargetDate = targetDate,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _goals.InsertAsync(goal);
    }

    public Task DeleteGoalAsync(int goalId) => _goals.DeleteByIdAsync(goalId);

    public async Task CompleteGoalAsync(int goalId)
    {
        var goal = await _goals.GetByIdAsync(goalId);
        if (goal is null) return;

        goal.CurrentValue = goal.TargetValue;
        goal.IsCompleted = true;
        goal.UpdatedAt = DateTime.UtcNow;
        await _goals.UpdateAsync(goal);
    }

    public async Task<IReadOnlyList<ExerciseOption>> GetAllExercisesAsync()
    {
        var exercises = await _exercises.GetAllAsync();
        return exercises
            .OrderBy(e => e.Name)
            .Select(e => new ExerciseOption(e.Id, e.Name))
            .ToList();
    }

    // ── Progress calculation ─────────────────────────────────────────────────

    private async Task RecalculateProgressAsync(IReadOnlyList<Goal> goals)
    {
        // Preload workout data once per recalculation pass
        var weekStart = GetStartOfWeek(DateTime.UtcNow);
        var allTimeWorkouts = await _workouts.GetCompletedInRangeAsync(DateTime.MinValue, DateTime.MaxValue);
        var thisWeekWorkouts = allTimeWorkouts
            .Where(w => w.CompletedAt >= weekStart)
            .ToList();

        // Balance: distinct muscle groups trained this week (compute once)
        int? muscleGroupsThisWeek = null;

        foreach (var goal in goals)
        {
            // Once completed (auto or manual), never revert — only recalculate progress value
            if (goal.IsCompleted)
            {
                // Still keep CurrentValue fresh so the progress text is accurate
                double completedCurrent = goal.Type switch
                {
                    GoalType.Strength => await CalculateStrengthCurrentAsync(goal.ExerciseId, allTimeWorkouts),
                    GoalType.Frequency => thisWeekWorkouts.Count,
                    GoalType.Balance => muscleGroupsThisWeek ??= await CalculateBalanceCurrentAsync(thisWeekWorkouts),
                    _ => goal.CurrentValue
                };
                if (Math.Abs(completedCurrent - goal.CurrentValue) > 0.001)
                {
                    goal.CurrentValue = completedCurrent;
                    goal.UpdatedAt = DateTime.UtcNow;
                    await _goals.UpdateAsync(goal);
                }
                continue;
            }

            double current = goal.Type switch
            {
                GoalType.Strength => await CalculateStrengthCurrentAsync(goal.ExerciseId, allTimeWorkouts),
                GoalType.Frequency => thisWeekWorkouts.Count,
                GoalType.Balance => muscleGroupsThisWeek ??= await CalculateBalanceCurrentAsync(thisWeekWorkouts),
                _ => 0
            };

            bool completed = goal.TargetValue > 0 && current >= goal.TargetValue;

            // Only update DB if the values changed
            if (Math.Abs(current - goal.CurrentValue) > 0.001 || goal.IsCompleted != completed)
            {
                goal.CurrentValue = current;
                goal.IsCompleted = completed;
                goal.UpdatedAt = DateTime.UtcNow;
                await _goals.UpdateAsync(goal);
            }
        }
    }

    private async Task<double> CalculateStrengthCurrentAsync(int? exerciseId, IReadOnlyList<Workout> allWorkouts)
    {
        if (!exerciseId.HasValue || allWorkouts.Count == 0) return 0;

        var workoutIds = allWorkouts.Select(w => w.Id).ToList();
        var workoutExercises = await _workoutExercises.GetByWorkoutIdsAsync(workoutIds);
        var matchingWeIds = workoutExercises
            .Where(we => we.ExerciseId == exerciseId.Value)
            .Select(we => we.Id)
            .ToList();

        if (matchingWeIds.Count == 0) return 0;

        double maxWeight = 0;
        foreach (var weId in matchingWeIds)
        {
            var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(weId);
            // Sets belong to already-completed workouts, so don't require IsCompleted on
            // individual sets — CompleteWorkoutAsync does not flip set flags.
            var localMax = sets
                .Where(s => s.Weight > 0 && s.Reps > 0)
                .Select(s => s.Weight)
                .DefaultIfEmpty(0)
                .Max();

            if (localMax > maxWeight)
                maxWeight = localMax;
        }

        return maxWeight;
    }

    private async Task<int> CalculateBalanceCurrentAsync(IReadOnlyList<Workout> thisWeekWorkouts)
    {
        if (thisWeekWorkouts.Count == 0) return 0;

        var workoutIds = thisWeekWorkouts.Select(w => w.Id).ToList();
        var workoutExercises = await _workoutExercises.GetByWorkoutIdsAsync(workoutIds);
        var exerciseIds = workoutExercises.Select(we => we.ExerciseId).ToHashSet();

        var allExercises = await _exercises.GetAllAsync();
        return allExercises
            .Where(e => exerciseIds.Contains(e.Id))
            .Select(e => e.PrimaryMuscle)
            .Distinct()
            .Count();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static GoalSummary BuildSummary(Goal goal, string? exerciseName)
    {
        double percent = goal.TargetValue > 0
            ? Math.Min(goal.CurrentValue / goal.TargetValue * 100.0, 100.0)
            : 0;

        double ratio = percent / 100.0;

        string progressText = goal.Type switch
        {
            GoalType.Strength => $"{goal.CurrentValue:F1} / {goal.TargetValue:F1} kg",
            GoalType.Frequency => $"{(int)goal.CurrentValue} / {(int)goal.TargetValue} workouts this week",
            GoalType.Balance => $"{(int)goal.CurrentValue} / {(int)goal.TargetValue} muscle groups",
            _ => string.Empty
        };

        string typeLabel = goal.Type switch
        {
            GoalType.Strength => "STRENGTH",
            GoalType.Frequency => "FREQUENCY",
            GoalType.Balance => "BALANCE",
            _ => string.Empty
        };

        string progressHint = goal.Type switch
        {
            GoalType.Strength => exerciseName is { Length: > 0 }
                ? $"Auto-tracks your max weight for {exerciseName}"
                : "Auto-tracks your max weight for the linked exercise",
            GoalType.Frequency => "Auto-tracks completed workouts this week (Mon – Sun)",
            GoalType.Balance => "Auto-tracks distinct primary muscle groups trained this week",
            _ => string.Empty
        };

        return new GoalSummary(
            Id: goal.Id,
            Type: goal.Type,
            Description: goal.Description,
            TargetValue: goal.TargetValue,
            CurrentValue: goal.CurrentValue,
            ProgressPercent: percent,
            ProgressRatio: ratio,
            ProgressText: progressText,
            TypeLabel: typeLabel,
            ExerciseName: exerciseName,
            ProgressHint: progressHint,
            IsCompleted: goal.IsCompleted,
            TargetDate: goal.TargetDate);
    }

    private static DateTime GetStartOfWeek(DateTime date)
    {
        // ISO week: Monday is the first day
        int daysSinceMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.Date.AddDays(-daysSinceMonday);
    }
}
