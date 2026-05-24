using Golyath.Application.DTOs;
using Golyath.Core.Abstractions;
using Golyath.Core.Enums;

namespace Golyath.Application.Services;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly IWorkoutRepository _workouts;
    private readonly IWorkoutExerciseRepository _workoutExercises;
    private readonly IWorkoutSetRepository _workoutSets;
    private readonly IExerciseRepository _exercises;

    public AnalyticsService(
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

    // ── Exercises with history ───────────────────────────────────────────────

    public async Task<IReadOnlyList<ExerciseOption>> GetExercisesWithHistoryAsync()
    {
        var workouts = await _workouts.GetCompletedInRangeAsync(DateTime.MinValue, DateTime.MaxValue);
        if (workouts.Count == 0) return [];

        var workoutIds = workouts.Select(w => w.Id).ToList();
        var workoutExercises = await _workoutExercises.GetByWorkoutIdsAsync(workoutIds);
        var exerciseIds = workoutExercises.Select(we => we.ExerciseId).ToHashSet();

        var allExercises = await _exercises.GetAllAsync();
        return allExercises
            .Where(e => exerciseIds.Contains(e.Id))
            .OrderBy(e => e.Name)
            .Select(e => new ExerciseOption(e.Id, e.Name))
            .ToList();
    }

    // ── Strength progression ─────────────────────────────────────────────────

    public async Task<StrengthProgressionData?> GetStrengthProgressionAsync(int exerciseId, DateTime from)
    {
        var exercise = await _exercises.GetByIdAsync(exerciseId);
        if (exercise is null) return null;

        var workouts = await _workouts.GetCompletedInRangeAsync(from, DateTime.UtcNow);
        if (workouts.Count == 0)
            return new StrengthProgressionData(exercise.Name, []);

        var points = new List<StrengthPoint>();

        foreach (var workout in workouts.OrderBy(w => w.CompletedAt))
        {
            var workoutExercises = await _workoutExercises.GetByWorkoutIdAsync(workout.Id);
            var match = workoutExercises.FirstOrDefault(we => we.ExerciseId == exerciseId);
            if (match is null) continue;

            var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(match.Id);
            double maxWeight = sets
                .Where(s => s.IsCompleted && s.Reps > 0)
                .Select(s => s.Weight)
                .DefaultIfEmpty(0)
                .Max();

            if (maxWeight > 0)
                points.Add(new StrengthPoint(workout.CompletedAt!.Value, maxWeight));
        }

        return new StrengthProgressionData(exercise.Name, points);
    }

    // ── Weekly volume ────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<VolumePoint>> GetWeeklyVolumeAsync(DateTime from)
    {
        var workouts = await _workouts.GetCompletedInRangeAsync(from, DateTime.UtcNow);
        if (workouts.Count == 0) return [];

        var volumeByWeek = new Dictionary<(int Year, int Week), double>();
        var labelByWeek = new Dictionary<(int Year, int Week), string>();

        foreach (var workout in workouts)
        {
            if (!workout.CompletedAt.HasValue) continue;

            var date = workout.CompletedAt.Value.ToLocalTime();
            var key = (date.Year, System.Globalization.ISOWeek.GetWeekOfYear(date));

            if (!labelByWeek.ContainsKey(key))
                labelByWeek[key] = GetMonday(date.Date).ToString("MMM d");

            var workoutExercises = await _workoutExercises.GetByWorkoutIdAsync(workout.Id);
            double workoutVolume = 0;
            foreach (var we in workoutExercises)
            {
                var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
                workoutVolume += sets.Where(s => s.IsCompleted).Sum(s => s.Weight * s.Reps);
            }

            volumeByWeek[key] = volumeByWeek.GetValueOrDefault(key, 0) + workoutVolume;
        }

        return volumeByWeek
            .OrderBy(kv => kv.Key.Year).ThenBy(kv => kv.Key.Week)
            .Select(kv => new VolumePoint(labelByWeek[kv.Key], kv.Value))
            .ToList();
    }

    // ── Muscle-group distribution ────────────────────────────────────────────

    public async Task<IReadOnlyList<MuscleGroupVolume>> GetMuscleGroupDistributionAsync(DateTime from)
    {
        var workouts = await _workouts.GetCompletedInRangeAsync(from, DateTime.UtcNow);
        if (workouts.Count == 0) return [];

        var setCounts = new Dictionary<MuscleGroup, int>();

        foreach (var workout in workouts)
        {
            var workoutExercises = await _workoutExercises.GetByWorkoutIdAsync(workout.Id);
            foreach (var we in workoutExercises)
            {
                var exercise = await _exercises.GetByIdAsync(we.ExerciseId);
                if (exercise is null) continue;

                var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
                int completedSets = sets.Count(s => s.IsCompleted);
                if (completedSets == 0) continue;

                setCounts[exercise.PrimaryMuscle] =
                    setCounts.GetValueOrDefault(exercise.PrimaryMuscle, 0) + completedSets;
            }
        }

        if (setCounts.Count == 0) return [];

        int total = setCounts.Values.Sum();
        return setCounts
            .OrderByDescending(kv => kv.Value)
            .Select(kv => new MuscleGroupVolume(
                MuscleGroupLabel(kv.Key),
                kv.Value,
                total > 0 ? (double)kv.Value / total : 0))
            .ToList();
    }

    // ── Muscle balance (5 fixed groups, relative to max) ─────────────────────

    private static readonly HashSet<MuscleGroup> LegMuscles =
        [MuscleGroup.Quads, MuscleGroup.Hamstrings, MuscleGroup.Glutes, MuscleGroup.Calves];

    public async Task<IReadOnlyList<MuscleBalanceItem>> GetMuscleBalanceAsync(DateTime from)
    {
        var setCounts = new Dictionary<string, int>
        {
            ["Chest"] = 0, ["Back"] = 0, ["Legs"] = 0, ["Shoulders"] = 0, ["Biceps"] = 0, ["Triceps"] = 0, ["Core"] = 0
        };

        var workouts = await _workouts.GetCompletedInRangeAsync(from, DateTime.UtcNow);
        foreach (var workout in workouts)
        {
            var workoutExercises = await _workoutExercises.GetByWorkoutIdAsync(workout.Id);
            foreach (var we in workoutExercises)
            {
                var exercise = await _exercises.GetByIdAsync(we.ExerciseId);
                if (exercise is null) continue;

                var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
                int completed = sets.Count(s => s.IsCompleted);
                if (completed == 0) continue;

                string? key = exercise.PrimaryMuscle switch
                {
                    MuscleGroup.Chest => "Chest",
                    MuscleGroup.Back => "Back",
                    MuscleGroup.Shoulders => "Shoulders",
                    MuscleGroup.Biceps or MuscleGroup.Forearms => "Biceps",
                    MuscleGroup.Triceps => "Triceps",
                    MuscleGroup.Abs => "Core",
                    var m when LegMuscles.Contains(m) => "Legs",
                    _ => null
                };
                if (key is not null) setCounts[key] += completed;
            }
        }

        int max = setCounts.Values.DefaultIfEmpty(0).Max();
        if (max == 0)
            return [
                new("Chest", 0), new("Back", 0), new("Legs", 0),
                new("Shoulders", 0), new("Biceps", 0), new("Triceps", 0), new("Core", 0)
            ];

        return [
            new("Chest",     (double)setCounts["Chest"]     / max),
            new("Back",      (double)setCounts["Back"]      / max),
            new("Legs",      (double)setCounts["Legs"]      / max),
            new("Shoulders", (double)setCounts["Shoulders"] / max),
            new("Biceps",    (double)setCounts["Biceps"]    / max),
            new("Triceps",   (double)setCounts["Triceps"]   / max),
            new("Core",      (double)setCounts["Core"]      / max),
        ];
    }

    // ── Recovery score ───────────────────────────────────────────────────────

    public async Task<int> GetRecoveryScoreAsync()
    {
        var now = DateTime.UtcNow;
        var recent = await _workouts.GetCompletedInRangeAsync(now.AddDays(-14), now);

        if (recent.Count == 0) return 100;

        var trainingDates = recent
            .Where(w => w.CompletedAt.HasValue)
            .Select(w => w.CompletedAt!.Value.ToLocalTime().Date)
            .ToHashSet();

        // Count consecutive training days going backwards from today
        int consecutive = 0;
        var day = DateTime.Now.Date;
        while (trainingDates.Contains(day) && consecutive <= 14)
        {
            consecutive++;
            day = day.AddDays(-1);
        }

        int score = Math.Max(15, 100 - consecutive * 15);

        // Partial recovery bonus for rest days since last workout
        var last = recent.OrderByDescending(w => w.CompletedAt).First();
        int restDays = (int)(now - last.CompletedAt!.Value).TotalDays;
        if (restDays >= 1) score = Math.Min(100, score + restDays * 10);

        return Math.Clamp(score, 15, 100);
    }

    // ── Intensity score ──────────────────────────────────────────────────────

    public async Task<int> GetIntensityScoreAsync(DateTime from)
    {
        var now = DateTime.UtcNow;
        var workouts = await _workouts.GetCompletedInRangeAsync(from, now);
        if (workouts.Count == 0) return 0;

        int totalSets = 0;
        foreach (var workout in workouts)
        {
            var wes = await _workoutExercises.GetByWorkoutIdAsync(workout.Id);
            foreach (var we in wes)
            {
                var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
                totalSets += sets.Count(s => s.IsCompleted);
            }
        }

        double avgSets = (double)totalSets / workouts.Count;
        double periodDays = Math.Max(1, (now - from).TotalDays);
        double sessionsPerWeek = workouts.Count / (periodDays / 7.0);

        double volumeScore  = Math.Min(1.0, avgSets / 16.0);   // 16 sets/session = max
        double freqScore    = Math.Min(1.0, sessionsPerWeek / 5.0); // 5/week = max
        double combined     = volumeScore * 0.65 + freqScore * 0.35;

        return (int)Math.Clamp(combined * 100, 0, 100);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static DateTime GetMonday(DateTime date)
    {
        int dow = (int)date.DayOfWeek;
        return date.AddDays(-(dow == 0 ? 6 : dow - 1));
    }

    private static string MuscleGroupLabel(MuscleGroup mg) => mg switch
    {
        MuscleGroup.Chest => "Chest",
        MuscleGroup.Back => "Back",
        MuscleGroup.Shoulders => "Shoulders",
        MuscleGroup.Biceps => "Biceps",
        MuscleGroup.Triceps => "Triceps",
        MuscleGroup.Forearms => "Forearms",
        MuscleGroup.Abs => "Abs",
        MuscleGroup.Quads => "Quads",
        MuscleGroup.Hamstrings => "Hamstrings",
        MuscleGroup.Glutes => "Glutes",
        MuscleGroup.Calves => "Calves",
        MuscleGroup.FullBody => "Full Body",
        _ => mg.ToString()
    };
}
