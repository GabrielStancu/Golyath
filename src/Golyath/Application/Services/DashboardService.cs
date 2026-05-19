using Golyath.Application.DTOs;
using Golyath.Core.Abstractions;
using Golyath.Core.Enums;

namespace Golyath.Application.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IWorkoutRepository _workouts;
    private readonly IWorkoutExerciseRepository _workoutExercises;
    private readonly IWorkoutSetRepository _workoutSets;
    private readonly IExerciseRepository _exercises;

    public DashboardService(
        IWorkoutRepository workouts,
        IWorkoutExerciseRepository workoutExercises,
        IWorkoutSetRepository workoutSets,
        IExerciseRepository exercises)
    {
        _workouts = workouts;
        _workoutExercises = workoutExercises;
        _workoutSets = workoutSets;
        _exercises = exercises;
    }

    public async Task<LastWorkoutSummary?> GetLastWorkoutSummaryAsync()
    {
        var last = await _workouts.GetLastCompletedAsync();
        if (last is null) return null;

        var exercises = await _workoutExercises.GetByWorkoutIdAsync(last.Id);

        int totalSets = 0;
        double totalVolume = 0;
        foreach (var we in exercises)
        {
            var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
            var done = sets.Where(s => s.IsCompleted).ToList();
            totalSets += done.Count;
            totalVolume += done.Sum(s => s.Weight * s.Reps);
        }

        return new LastWorkoutSummary(
            last.Name,
            last.CompletedAt!.Value,
            exercises.Count,
            totalSets,
            totalVolume,
            last.DurationSeconds);
    }

    public async Task<IReadOnlyList<WeeklyActivityDay>> GetWeeklyActivityAsync()
    {
        var today = DateTime.UtcNow.Date;
        // ISO week: Monday = start
        var dow = (int)today.DayOfWeek;
        var monday = today.AddDays(-(dow == 0 ? 6 : dow - 1));
        var sunday = monday.AddDays(7).AddSeconds(-1);

        var workoutsThisWeek = await _workouts.GetCompletedInRangeAsync(monday, sunday);

        var workoutDays = workoutsThisWeek
            .Where(w => w.CompletedAt.HasValue)
            .Select(w => w.CompletedAt!.Value.Date)
            .ToHashSet();

        string[] labels = ["M", "T", "W", "T", "F", "S", "S"];
        return Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = monday.AddDays(i);
                return new WeeklyActivityDay(labels[i], workoutDays.Contains(date), date == today);
            })
            .ToList();
    }

    public async Task<ReadinessInfo> GetReadinessAsync()
    {
        var last = await _workouts.GetLastCompletedAsync();
        if (last is null)
        {
            return new ReadinessInfo(ReadinessLevel.Ready, "Fresh Start",
                "No workouts yet — this is the perfect time to begin!", 999);
        }

        var daysSince = (DateTime.UtcNow.Date - last.CompletedAt!.Value.ToUniversalTime().Date).Days;

        return daysSince switch
        {
            0 => new ReadinessInfo(ReadinessLevel.Rest, "Rest Day",
                "Great session today! Let your muscles recover.", 0),
            1 => new ReadinessInfo(ReadinessLevel.Moderate, "Recovering",
                "One day of rest — listen to your body.", 1),
            _ => new ReadinessInfo(ReadinessLevel.Ready, "Ready to Train",
                $"You've rested for {daysSince} days. Time to crush it!", daysSince)
        };
    }

    public async Task<WorkoutSuggestion> GetWorkoutSuggestionAsync()
    {
        var twoWeeksAgo = DateTime.UtcNow.AddDays(-14);
        var recentWorkouts = await _workouts.GetCompletedInRangeAsync(twoWeeksAgo, DateTime.UtcNow);

        if (recentWorkouts.Count == 0)
        {
            return new WorkoutSuggestion(MuscleGroup.Chest,
                "Start with a classic — chest is a great foundation.");
        }

        var workoutIds = recentWorkouts.Select(w => w.Id).ToList();
        var recentExercises = await _workoutExercises.GetByWorkoutIdsAsync(workoutIds);

        // Count how many times each muscle group was the primary focus
        var muscleCounts = new Dictionary<MuscleGroup, int>();
        var uniqueExerciseIds = recentExercises.Select(we => we.ExerciseId).Distinct();

        foreach (var exerciseId in uniqueExerciseIds)
        {
            var exercise = await _exercises.GetByIdAsync(exerciseId);
            if (exercise is null) continue;
            muscleCounts.TryGetValue(exercise.PrimaryMuscle, out var count);
            muscleCounts[exercise.PrimaryMuscle] = count + 1;
        }

        // Find the least-trained muscle group (excluding FullBody)
        var allMuscles = Enum.GetValues<MuscleGroup>()
            .Where(m => m != MuscleGroup.FullBody)
            .ToList();

        var leastTrained = allMuscles
            .OrderBy(m => muscleCounts.GetValueOrDefault(m, 0))
            .First();

        var reason = muscleCounts.ContainsKey(leastTrained)
            ? $"Your {leastTrained} muscles could use more attention this week."
            : $"You haven't trained {leastTrained} recently — give it some love.";

        return new WorkoutSuggestion(leastTrained, reason);
    }

    public async Task<int> GetWeeklyWorkoutCountAsync()
    {
        var today = DateTime.UtcNow.Date;
        var dow = (int)today.DayOfWeek;
        var monday = today.AddDays(-(dow == 0 ? 6 : dow - 1));
        var sunday = monday.AddDays(7).AddSeconds(-1);

        var workouts = await _workouts.GetCompletedInRangeAsync(monday, sunday);
        return workouts.Count;
    }

    public async Task<double> GetWeeklyVolumeAsync()
    {
        var today = DateTime.UtcNow.Date;
        var dow = (int)today.DayOfWeek;
        var monday = today.AddDays(-(dow == 0 ? 6 : dow - 1));
        var sunday = monday.AddDays(7).AddSeconds(-1);

        var workouts = await _workouts.GetCompletedInRangeAsync(monday, sunday);
        double totalVolume = 0;

        foreach (var w in workouts)
        {
            var exercises = await _workoutExercises.GetByWorkoutIdAsync(w.Id);
            foreach (var we in exercises)
            {
                var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
                totalVolume += sets.Where(s => s.IsCompleted).Sum(s => s.Weight * s.Reps);
            }
        }

        return totalVolume;
    }

    public async Task<int> GetWeekStreakAsync()
    {
        // Count consecutive weeks (ending with current or last week) that have at least one workout.
        int streak = 0;
        var today = DateTime.UtcNow.Date;
        var dow = (int)today.DayOfWeek;
        var currentMonday = today.AddDays(-(dow == 0 ? 6 : dow - 1));

        for (int i = 0; i < 52; i++) // max 1 year lookback
        {
            var weekStart = currentMonday.AddDays(-7 * i);
            var weekEnd = weekStart.AddDays(7).AddSeconds(-1);
            var workouts = await _workouts.GetCompletedInRangeAsync(weekStart, weekEnd);

            if (workouts.Count > 0)
            {
                streak++;
            }
            else
            {
                // If we're on the current week and no workouts yet, skip and check previous weeks
                if (i == 0) continue;
                break;
            }
        }

        return streak;
    }
}
