using Golyath.Application.DTOs;
using Golyath.Core.Abstractions;
using Golyath.Core.Enums;

namespace Golyath.Application.Services;

public sealed class SuggestionsService : ISuggestionsService
{
    // ── Configuration constants ──────────────────────────────────────────────

    /// <summary>Number of consecutive sessions at the same weight/reps that signal a plateau.</summary>
    private const int PlateauSessionThreshold = 4;

    /// <summary>Minimum sessions an exercise must have before plateau analysis runs.</summary>
    private const int MinSessionsForPlateauAnalysis = PlateauSessionThreshold;

    /// <summary>Push:pull ratio above this threshold triggers a push-heavy imbalance warning.</summary>
    private const double ImbalanceRatioThreshold = 2.0;

    /// <summary>Look-back window in days for imbalance and undertrained analysis.</summary>
    private const int AnalysisWindowDays = 28; // 4 weeks

    /// <summary>Minimum workouts within the analysis window before undertrained detection fires.</summary>
    private const int MinWorkoutsForUndertrainedCheck = 4;

    /// <summary>Workouts in the last 7 days above this count trigger a deload recommendation.</summary>
    private const int DeloadWorkoutFrequencyThreshold = 5;

    /// <summary>Number of consecutive weeks of increasing volume that triggers a volume-based deload.</summary>
    private const int DeloadConsecutiveWeeksThreshold = 3;

    // ── Muscle group sets ────────────────────────────────────────────────────

    private static readonly HashSet<MuscleGroup> PushMuscles =
        [MuscleGroup.Chest, MuscleGroup.Shoulders, MuscleGroup.Triceps];

    private static readonly HashSet<MuscleGroup> PullMuscles =
        [MuscleGroup.Back, MuscleGroup.Biceps];

    private static readonly MuscleGroup[] MajorGroups =
    [
        MuscleGroup.Chest,
        MuscleGroup.Back,
        MuscleGroup.Shoulders,
        MuscleGroup.Quads,
        MuscleGroup.Hamstrings,
        MuscleGroup.Abs
    ];

    // ── Dependencies ─────────────────────────────────────────────────────────

    private readonly IWorkoutRepository _workouts;
    private readonly IWorkoutExerciseRepository _workoutExercises;
    private readonly IWorkoutSetRepository _workoutSets;
    private readonly IExerciseRepository _exercises;

    public SuggestionsService(
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

    // ── Public API ───────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<TrainingSuggestion>> GetSuggestionsAsync()
    {
        var suggestions = new List<TrainingSuggestion>();

        var now = DateTime.UtcNow;
        var windowStart = now.AddDays(-AnalysisWindowDays);

        // Load shared data
        var recentWorkouts = await _workouts.GetCompletedInRangeAsync(windowStart, now);
        var allCompletedWorkouts = await _workouts.GetCompletedInRangeAsync(DateTime.MinValue, now);

        if (allCompletedWorkouts.Count == 0)
            return suggestions;

        // Run each detector
        suggestions.AddRange(await DetectDeloadNeedAsync(recentWorkouts, allCompletedWorkouts, now));
        suggestions.AddRange(await DetectPlateausAsync(allCompletedWorkouts));
        suggestions.AddRange(await DetectMuscleImbalanceAsync(recentWorkouts));
        suggestions.AddRange(await DetectUndertrainedMusclesAsync(recentWorkouts));

        // Sort: High priority first, then by type ordinal for stable ordering
        suggestions.Sort((a, b) =>
        {
            int priorityComp = b.Priority.CompareTo(a.Priority);
            return priorityComp != 0 ? priorityComp : a.Type.CompareTo(b.Type);
        });

        return suggestions;
    }

    // ── Deload detection ─────────────────────────────────────────────────────

    private async Task<IEnumerable<TrainingSuggestion>> DetectDeloadNeedAsync(
        IReadOnlyList<Core.Entities.Workout> recentWorkouts,
        IReadOnlyList<Core.Entities.Workout> allWorkouts,
        DateTime now)
    {
        var suggestions = new List<TrainingSuggestion>();

        // Trigger 1: frequency — 5+ workouts in last 7 days
        var last7Days = recentWorkouts
            .Where(w => w.CompletedAt >= now.AddDays(-7))
            .ToList();

        if (last7Days.Count >= DeloadWorkoutFrequencyThreshold)
        {
            suggestions.Add(new TrainingSuggestion(
                SuggestionType.Deload,
                SuggestionPriority.High,
                "Consider a Deload Week",
                $"You completed {last7Days.Count} workouts in the last 7 days. High frequency without rest increases injury risk. Try a lighter week with reduced volume.",
                "🛑"));
            return suggestions; // One deload suggestion is enough
        }

        // Trigger 2: 3+ consecutive weeks of increasing volume
        var weeklyVolumes = await ComputeWeeklyVolumesAsync(allWorkouts, DeloadConsecutiveWeeksThreshold + 1);

        if (weeklyVolumes.Count >= DeloadConsecutiveWeeksThreshold)
        {
            bool consecutiveIncrease = true;
            for (int i = weeklyVolumes.Count - 1; i >= weeklyVolumes.Count - DeloadConsecutiveWeeksThreshold; i--)
            {
                if (i == 0 || weeklyVolumes[i] <= weeklyVolumes[i - 1])
                {
                    consecutiveIncrease = false;
                    break;
                }
            }

            if (consecutiveIncrease)
            {
                suggestions.Add(new TrainingSuggestion(
                    SuggestionType.Deload,
                    SuggestionPriority.Medium,
                    "Volume Has Been Rising for 3+ Weeks",
                    $"Your training volume has increased every week for the last {DeloadConsecutiveWeeksThreshold} weeks. A planned deload will help your body recover and prevent overtraining.",
                    "📉"));
            }
        }

        return suggestions;
    }

    // ── Plateau detection ────────────────────────────────────────────────────

    private async Task<IEnumerable<TrainingSuggestion>> DetectPlateausAsync(
        IReadOnlyList<Core.Entities.Workout> allWorkouts)
    {
        var suggestions = new List<TrainingSuggestion>();

        // Build a map: exerciseId → list of (sessionDate, maxWeight, maxReps)
        var exerciseSessions = new Dictionary<int, List<(DateTime date, double maxWeight, int maxReps)>>();

        foreach (var workout in allWorkouts.OrderBy(w => w.CompletedAt))
        {
            var wes = await _workoutExercises.GetByWorkoutIdAsync(workout.Id);
            foreach (var we in wes)
            {
                var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
                var completedSets = sets.Where(s => s.IsCompleted && s.Reps > 0).ToList();
                if (completedSets.Count == 0) continue;

                double maxWeight = completedSets.Max(s => s.Weight);
                int maxReps = completedSets.Max(s => s.Reps);

                if (!exerciseSessions.ContainsKey(we.ExerciseId))
                    exerciseSessions[we.ExerciseId] = [];

                exerciseSessions[we.ExerciseId].Add((workout.CompletedAt!.Value, maxWeight, maxReps));
            }
        }

        // Analyse exercises with enough sessions
        foreach (var (exerciseId, sessions) in exerciseSessions)
        {
            if (sessions.Count < MinSessionsForPlateauAnalysis) continue;

            var lastN = sessions
                .OrderBy(s => s.date)
                .TakeLast(PlateauSessionThreshold)
                .ToList();

            double firstWeight = lastN[0].maxWeight;
            int firstReps = lastN[0].maxReps;

            bool weightStagnant = lastN.All(s => Math.Abs(s.maxWeight - firstWeight) < 0.01);
            bool repsStagnant = lastN.All(s => s.maxReps == firstReps);

            if (!weightStagnant || !repsStagnant) continue;

            var exercise = await _exercises.GetByIdAsync(exerciseId);
            if (exercise is null) continue;

            suggestions.Add(new TrainingSuggestion(
                SuggestionType.PlateauDetected,
                SuggestionPriority.Medium,
                $"Plateau Detected: {exercise.Name}",
                $"Your last {PlateauSessionThreshold} sessions all used {firstWeight:0.#} kg × {firstReps} reps. Try adding 2.5 kg or 1–2 extra reps to break through.",
                "📊"));
        }

        return suggestions;
    }

    // ── Muscle imbalance detection ───────────────────────────────────────────

    private async Task<IEnumerable<TrainingSuggestion>> DetectMuscleImbalanceAsync(
        IReadOnlyList<Core.Entities.Workout> recentWorkouts)
    {
        var suggestions = new List<TrainingSuggestion>();

        if (recentWorkouts.Count == 0) return suggestions;

        int pushSets = 0;
        int pullSets = 0;

        foreach (var workout in recentWorkouts)
        {
            var wes = await _workoutExercises.GetByWorkoutIdAsync(workout.Id);
            foreach (var we in wes)
            {
                var exercise = await _exercises.GetByIdAsync(we.ExerciseId);
                if (exercise is null) continue;

                var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
                int completedSets = sets.Count(s => s.IsCompleted);
                if (completedSets == 0) continue;

                if (PushMuscles.Contains(exercise.PrimaryMuscle))
                    pushSets += completedSets;
                else if (PullMuscles.Contains(exercise.PrimaryMuscle))
                    pullSets += completedSets;
            }
        }

        // Ratio is only meaningful when both sides have data
        if (pushSets == 0 || pullSets == 0) return suggestions;

        double pushToPull = (double)pushSets / pullSets;
        double pullToPush = (double)pullSets / pushSets;

        if (pushToPull >= ImbalanceRatioThreshold)
        {
            suggestions.Add(new TrainingSuggestion(
                SuggestionType.MuscleImbalance,
                SuggestionPriority.Medium,
                "Push-Heavy Imbalance",
                $"Over the last 4 weeks you logged {pushSets} push sets vs {pullSets} pull sets ({pushToPull:0.#}:1 ratio). Add more rows, pull-downs, or face pulls to balance your training.",
                "⚖️"));
        }
        else if (pullToPush >= ImbalanceRatioThreshold)
        {
            suggestions.Add(new TrainingSuggestion(
                SuggestionType.MuscleImbalance,
                SuggestionPriority.Medium,
                "Pull-Heavy Imbalance",
                $"Over the last 4 weeks you logged {pullSets} pull sets vs {pushSets} push sets ({pullToPush:0.#}:1 ratio). Add more presses or flyes to balance your training.",
                "⚖️"));
        }

        return suggestions;
    }

    // ── Undertrained muscle detection ────────────────────────────────────────

    private async Task<IEnumerable<TrainingSuggestion>> DetectUndertrainedMusclesAsync(
        IReadOnlyList<Core.Entities.Workout> recentWorkouts)
    {
        var suggestions = new List<TrainingSuggestion>();

        if (recentWorkouts.Count < MinWorkoutsForUndertrainedCheck) return suggestions;

        var trainedMuscles = new HashSet<MuscleGroup>();

        foreach (var workout in recentWorkouts)
        {
            var wes = await _workoutExercises.GetByWorkoutIdAsync(workout.Id);
            foreach (var we in wes)
            {
                var exercise = await _exercises.GetByIdAsync(we.ExerciseId);
                if (exercise is null) continue;

                var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
                if (sets.Any(s => s.IsCompleted))
                    trainedMuscles.Add(exercise.PrimaryMuscle);
            }
        }

        foreach (var muscle in MajorGroups)
        {
            if (!trainedMuscles.Contains(muscle))
            {
                suggestions.Add(new TrainingSuggestion(
                    SuggestionType.UndertrainedMuscle,
                    SuggestionPriority.Low,
                    $"{muscle} Not Trained in 4 Weeks",
                    $"You haven't logged any {muscle.ToString().ToLower()} exercises in the last 4 weeks. Consider adding a {muscle.ToString().ToLower()} movement to maintain balanced development.",
                    "💡"));
            }
        }

        return suggestions;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Returns weekly total volumes (weight × reps) for the most recent <paramref name="weeksBack"/> weeks, oldest first.</summary>
    private async Task<List<double>> ComputeWeeklyVolumesAsync(
        IReadOnlyList<Core.Entities.Workout> allWorkouts,
        int weeksBack)
    {
        var now = DateTime.UtcNow;
        var weeklyVolumes = new List<double>(weeksBack);

        for (int w = weeksBack - 1; w >= 0; w--)
        {
            var weekStart = now.AddDays(-7 * (w + 1));
            var weekEnd = now.AddDays(-7 * w);

            double volume = 0;
            foreach (var workout in allWorkouts.Where(x => x.CompletedAt >= weekStart && x.CompletedAt < weekEnd))
            {
                var wes = await _workoutExercises.GetByWorkoutIdAsync(workout.Id);
                foreach (var we in wes)
                {
                    var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
                    volume += sets.Where(s => s.IsCompleted).Sum(s => s.Weight * s.Reps);
                }
            }

            weeklyVolumes.Add(volume);
        }

        return weeklyVolumes;
    }
}
