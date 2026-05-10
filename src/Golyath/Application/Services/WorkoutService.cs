using Golyath.Core.Abstractions;
using Golyath.Core.Entities;

namespace Golyath.Application.Services;

public sealed class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IWorkoutExerciseRepository _workoutExerciseRepository;
    private readonly IWorkoutSetRepository _workoutSetRepository;

    public WorkoutService(
        IWorkoutRepository workoutRepository,
        IWorkoutExerciseRepository workoutExerciseRepository,
        IWorkoutSetRepository workoutSetRepository)
    {
        _workoutRepository = workoutRepository;
        _workoutExerciseRepository = workoutExerciseRepository;
        _workoutSetRepository = workoutSetRepository;
    }

    public async Task<Workout> StartWorkoutAsync(string? name = null)
    {
        var workout = new Workout
        {
            Name = name,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        await _workoutRepository.InsertAsync(workout);
        return workout;
    }

    public Task<Workout?> GetActiveWorkoutAsync() =>
        _workoutRepository.GetActiveWorkoutAsync();

    public async Task<WorkoutExercise> AddExerciseAsync(int workoutId, int exerciseId)
    {
        var existing = await _workoutExerciseRepository.GetByWorkoutIdAsync(workoutId);
        var we = new WorkoutExercise
        {
            WorkoutId = workoutId,
            ExerciseId = exerciseId,
            Order = existing.Count
        };
        await _workoutExerciseRepository.InsertAsync(we);
        return we;
    }

    public Task RemoveExerciseAsync(int workoutExerciseId) =>
        _workoutExerciseRepository.DeleteByIdAsync(workoutExerciseId);

    public async Task<WorkoutSet> AddSetAsync(int workoutExerciseId, double weight, int reps,
        string? tempo = null, string? notes = null)
    {
        var existing = await _workoutSetRepository.GetByWorkoutExerciseIdAsync(workoutExerciseId);
        var set = new WorkoutSet
        {
            WorkoutExerciseId = workoutExerciseId,
            SetNumber = existing.Count + 1,
            Weight = weight,
            Reps = reps,
            Tempo = tempo,
            Notes = notes,
            IsCompleted = false
        };
        await _workoutSetRepository.InsertAsync(set);
        return set;
    }

    public async Task<WorkoutSet> CompleteSetAsync(int setId)
    {
        var set = await _workoutSetRepository.GetByIdAsync(setId)
            ?? throw new InvalidOperationException($"Set {setId} not found.");
        set.IsCompleted = true;
        set.CompletedAt = DateTime.UtcNow;
        await _workoutSetRepository.UpdateAsync(set);
        return set;
    }

    public async Task<WorkoutSet> DuplicateSetAsync(int setId)
    {
        var original = await _workoutSetRepository.GetByIdAsync(setId)
            ?? throw new InvalidOperationException($"Set {setId} not found.");
        var existing = await _workoutSetRepository.GetByWorkoutExerciseIdAsync(original.WorkoutExerciseId);
        var newSet = new WorkoutSet
        {
            WorkoutExerciseId = original.WorkoutExerciseId,
            SetNumber = existing.Count + 1,
            Weight = original.Weight,
            Reps = original.Reps,
            Tempo = original.Tempo,
            IsCompleted = false
        };
        await _workoutSetRepository.InsertAsync(newSet);
        return newSet;
    }

    public Task UpdateSetAsync(WorkoutSet set) =>
        _workoutSetRepository.UpdateAsync(set);

    public async Task UpdateSetFieldsAsync(int setId, double weight, int reps, string? tempo, string? notes)
    {
        var set = await _workoutSetRepository.GetByIdAsync(setId);
        if (set is null) return;
        set.Weight = weight;
        set.Reps = reps;
        set.Tempo = string.IsNullOrWhiteSpace(tempo) ? null : tempo;
        set.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
        await _workoutSetRepository.UpdateAsync(set);
    }

    public async Task CompleteWorkoutAsync(int workoutId)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId)
            ?? throw new InvalidOperationException($"Workout {workoutId} not found.");
        workout.CompletedAt = DateTime.UtcNow;
        workout.DurationSeconds = (int)(workout.CompletedAt.Value - workout.StartedAt).TotalSeconds;
        await _workoutRepository.UpdateAsync(workout);
    }

    public async Task AbandonWorkoutAsync(int workoutId)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId);
        if (workout is not null)
            await _workoutRepository.DeleteAsync(workout);
    }

    public async Task UpdateWorkoutNotesAsync(int workoutId, string? notes)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId);
        if (workout is null) return;
        workout.Notes = notes;
        await _workoutRepository.UpdateAsync(workout);
    }

    public async Task UpdateExerciseNotesAsync(int workoutExerciseId, string? notes)
    {
        var we = await _workoutExerciseRepository.GetByIdAsync(workoutExerciseId);
        if (we is null) return;
        we.Notes = notes;
        await _workoutExerciseRepository.UpdateAsync(we);
    }

    public Task<IReadOnlyList<WorkoutExercise>> GetWorkoutExercisesAsync(int workoutId) =>
        _workoutExerciseRepository.GetByWorkoutIdAsync(workoutId);

    public Task<IReadOnlyList<WorkoutSet>> GetSetsForExerciseAsync(int workoutExerciseId) =>
        _workoutSetRepository.GetByWorkoutExerciseIdAsync(workoutExerciseId);

    public Task<WorkoutSet?> GetLastSetForAutofillAsync(int exerciseId) =>
        _workoutSetRepository.GetLastCompletedSetForExerciseAsync(exerciseId);
}
