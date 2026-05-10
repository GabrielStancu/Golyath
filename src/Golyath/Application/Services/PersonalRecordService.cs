using Golyath.Application.DTOs;
using Golyath.Core.Abstractions;

namespace Golyath.Application.Services;

public sealed class PersonalRecordService : IPersonalRecordService
{
    private readonly IWorkoutRepository _workouts;
    private readonly IWorkoutExerciseRepository _workoutExercises;
    private readonly IWorkoutSetRepository _workoutSets;
    private readonly IExerciseRepository _exercises;

    public PersonalRecordService(
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

    /// <inheritdoc />
    public async Task<IReadOnlyList<PersonalRecord>> GetPersonalRecordsAsync(int userId)
    {
        // Load all completed workouts (app is single-user; userId is kept for future-proofing)
        var workouts = await _workouts.GetCompletedInRangeAsync(DateTime.MinValue, DateTime.MaxValue);
        if (workouts.Count == 0) return [];

        var workoutIds = workouts.Select(w => w.Id).ToList();
        var allWorkoutExercises = await _workoutExercises.GetByWorkoutIdsAsync(workoutIds);

        // Group workout-exercises by exercise id to batch set loading
        var byExercise = allWorkoutExercises.GroupBy(we => we.ExerciseId);

        // Build a lookup: workoutId → CompletedAt for timestamp attribution
        var completedAtByWorkoutId = workouts
            .Where(w => w.CompletedAt.HasValue)
            .ToDictionary(w => w.Id, w => w.CompletedAt!.Value);

        var records = new List<PersonalRecord>();

        foreach (var group in byExercise)
        {
            int exerciseId = group.Key;

            // Collect all completed sets across every workout this exercise appeared in
            var allSets = new List<(double Weight, int Reps, DateTime CompletedAt)>();

            foreach (var we in group)
            {
                if (!completedAtByWorkoutId.TryGetValue(we.WorkoutId, out var workoutDate))
                    continue;

                var sets = await _workoutSets.GetByWorkoutExerciseIdAsync(we.Id);
                foreach (var s in sets)
                {
                    if (!s.IsCompleted || s.Reps <= 0)
                        continue;

                    allSets.Add((s.Weight, s.Reps, s.CompletedAt ?? workoutDate));
                }
            }

            if (allSets.Count == 0)
                continue;

            double maxWeight = allSets.Max(s => s.Weight);
            int maxReps = allSets.Max(s => s.Reps);
            double maxVolume = allSets.Max(s => s.Weight * s.Reps);

            // Epley 1RM estimate: weight × (1 + reps / 30)
            var best1RMSet = allSets
                .Select(s => (EstimatedOneRM: s.Weight * (1.0 + s.Reps / 30.0), s.CompletedAt))
                .OrderByDescending(x => x.EstimatedOneRM)
                .First();

            var exercise = await _exercises.GetByIdAsync(exerciseId);
            if (exercise is null) continue;

            records.Add(new PersonalRecord(
                ExerciseId: exerciseId,
                ExerciseName: exercise.Name,
                MaxWeight: maxWeight,
                MaxReps: maxReps,
                MaxVolume: maxVolume,
                EstimatedOneRM: Math.Round(best1RMSet.EstimatedOneRM, 1),
                AchievedAt: best1RMSet.CompletedAt));
        }

        return records
            .OrderBy(r => r.ExerciseName)
            .ToList();
    }
}
