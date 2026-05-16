using Golyath.Application.DTOs;
using Golyath.Core.Abstractions;
using Golyath.Core.Entities;

namespace Golyath.Application.Services;

public sealed class WorkoutHistoryService : IWorkoutHistoryService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IWorkoutExerciseRepository _workoutExerciseRepository;
    private readonly IWorkoutSetRepository _workoutSetRepository;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IWorkoutTagRepository _workoutTagRepository;
    private readonly IWorkoutService _workoutService;

    public WorkoutHistoryService(
        IWorkoutRepository workoutRepository,
        IWorkoutExerciseRepository workoutExerciseRepository,
        IWorkoutSetRepository workoutSetRepository,
        IExerciseRepository exerciseRepository,
        ITagRepository tagRepository,
        IWorkoutTagRepository workoutTagRepository,
        IWorkoutService workoutService)
    {
        _workoutRepository = workoutRepository;
        _workoutExerciseRepository = workoutExerciseRepository;
        _workoutSetRepository = workoutSetRepository;
        _exerciseRepository = exerciseRepository;
        _tagRepository = tagRepository;
        _workoutTagRepository = workoutTagRepository;
        _workoutService = workoutService;
    }

    public async Task<IReadOnlyList<WorkoutHistorySummaryDto>> GetHistoryAsync(
        DateTime? from = null,
        DateTime? to = null,
        int? tagId = null)
    {
        // Resolve the date window
        var rangeFrom = from ?? DateTime.MinValue;
        var rangeTo = to ?? DateTime.MaxValue;

        IReadOnlyList<Workout> workouts = tagId.HasValue
            ? await GetWorkoutsForTagInRangeAsync(tagId.Value, rangeFrom, rangeTo)
            : await _workoutRepository.GetCompletedInRangeAsync(rangeFrom, rangeTo);

        if (workouts.Count == 0)
            return [];

        // Batch-load exercises, sets, and tags for all returned workouts
        var workoutIds = workouts.Select(w => w.Id).ToList();
        var allExercises = await _workoutExerciseRepository.GetByWorkoutIdsAsync(workoutIds);
        var exerciseWeIds = allExercises.Select(we => we.Id).ToHashSet();

        // Load sets per workout-exercise — group them
        var allSets = new List<WorkoutSet>();
        foreach (var we in allExercises)
        {
            var sets = await _workoutSetRepository.GetByWorkoutExerciseIdAsync(we.Id);
            allSets.AddRange(sets.Where(s => s.IsCompleted));
        }

        var setsByWeId = allSets.GroupBy(s => s.WorkoutExerciseId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var exercisesByWorkoutId = allExercises.GroupBy(we => we.WorkoutId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var summaries = new List<WorkoutHistorySummaryDto>(workouts.Count);
        foreach (var workout in workouts)
        {
            var wes = exercisesByWorkoutId.GetValueOrDefault(workout.Id, []);
            var setCount = wes.Sum(we => setsByWeId.GetValueOrDefault(we.Id, []).Count);
            var totalVolume = wes
                .SelectMany(we => setsByWeId.GetValueOrDefault(we.Id, []))
                .Sum(s => s.Weight * s.Reps);

            var tags = await _workoutTagRepository.GetTagsForWorkoutAsync(workout.Id);
            var tagNames = tags.Select(t => t.Name).ToList();

            summaries.Add(new WorkoutHistorySummaryDto(
                Id: workout.Id,
                DisplayName: workout.Name ?? $"Workout — {workout.StartedAt.ToLocalTime():MMM d}",
                CompletedAt: workout.CompletedAt!.Value,
                DurationSeconds: workout.DurationSeconds,
                ExerciseCount: wes.Count,
                SetCount: setCount,
                TotalVolumeKg: totalVolume,
                TagNames: tagNames));
        }

        return summaries;
    }

    public async Task<WorkoutHistoryDetailDto?> GetWorkoutDetailAsync(int workoutId)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId);
        if (workout?.CompletedAt is null)
            return null;

        var workoutExercises = await _workoutExerciseRepository.GetByWorkoutIdAsync(workoutId);
        var tags = await _workoutTagRepository.GetTagsForWorkoutAsync(workoutId);

        // Build exercise summaries — load exercises to get their names
        var exerciseSummaries = new List<WorkoutExerciseSummaryDto>(workoutExercises.Count);
        foreach (var we in workoutExercises)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(we.ExerciseId);
            var sets = await _workoutSetRepository.GetByWorkoutExerciseIdAsync(we.Id);
            var completedSets = sets.Where(s => s.IsCompleted).ToList();

            var setDtos = completedSets.Select(s => new SetSummaryDto(
                SetId: s.Id,
                SetNumber: s.SetNumber,
                Weight: s.Weight,
                Reps: s.Reps,
                Tempo: s.Tempo,
                Notes: s.Notes)).ToList();

            exerciseSummaries.Add(new WorkoutExerciseSummaryDto(
                WorkoutExerciseId: we.Id,
                ExerciseName: exercise?.Name ?? "Unknown exercise",
                ExerciseNotes: we.Notes,
                Sets: setDtos));
        }

        return new WorkoutHistoryDetailDto(
            Id: workout.Id,
            DisplayName: workout.Name ?? $"Workout — {workout.StartedAt.ToLocalTime():MMM d}",
            CompletedAt: workout.CompletedAt.Value,
            DurationSeconds: workout.DurationSeconds,
            Notes: workout.Notes,
            Exercises: exerciseSummaries,
            TagNames: tags.Select(t => t.Name).ToList());
    }

    public Task<IReadOnlyList<Tag>> GetAllTagsAsync() =>
        _tagRepository.GetAllAsync();

    public Task<IReadOnlyList<Tag>> GetTagsForWorkoutAsync(int workoutId) =>
        _workoutTagRepository.GetTagsForWorkoutAsync(workoutId);

    public async Task<Tag> GetOrCreateTagAsync(string name)
    {
        var existing = await _tagRepository.GetByNameAsync(name);
        if (existing is not null)
            return existing;

        var tag = new Tag { Name = name };
        await _tagRepository.InsertAsync(tag);
        return tag;
    }

    public Task AssignTagAsync(int workoutId, int tagId) =>
        _workoutTagRepository.AddAsync(workoutId, tagId);

    public Task RemoveTagAsync(int workoutId, int tagId) =>
        _workoutTagRepository.RemoveAsync(workoutId, tagId);

    public Task DeleteWorkoutAsync(int workoutId) =>
        _workoutService.DeleteWorkoutAsync(workoutId);

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<IReadOnlyList<Workout>> GetWorkoutsForTagInRangeAsync(
        int tagId, DateTime from, DateTime to)
    {
        var workoutIds = await _workoutTagRepository.GetWorkoutIdsForTagAsync(tagId);
        if (workoutIds.Count == 0)
            return [];

        var range = await _workoutRepository.GetCompletedInRangeAsync(from, to);
        var idSet = workoutIds.ToHashSet();
        return range.Where(w => idSet.Contains(w.Id)).ToList();
    }
}
