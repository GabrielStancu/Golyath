using Golyath.Core.Entities;

namespace Golyath.Application.Services;

public interface IWorkoutService
{
    Task<Workout> StartWorkoutAsync(string? name = null);
    Task<Workout> StartWorkoutFromRoutineAsync(int routineId);
    Task<Workout?> GetActiveWorkoutAsync();
    Task<WorkoutExercise> AddExerciseAsync(int workoutId, int exerciseId);
    Task RemoveExerciseAsync(int workoutExerciseId);
    Task RemoveSetAsync(int setId);
    Task<WorkoutSet> AddSetAsync(int workoutExerciseId, double weight, int reps, string? tempo = null, string? notes = null);
    Task<WorkoutSet> CompleteSetAsync(int setId);
    Task<WorkoutSet> DuplicateSetAsync(int setId);
    Task UpdateSetAsync(WorkoutSet set);
    Task UpdateSetFieldsAsync(int setId, double weight, int reps, string? tempo, string? notes);
    Task CompleteWorkoutAsync(int workoutId);
    Task AbandonWorkoutAsync(int workoutId);
    Task DeleteWorkoutAsync(int workoutId);
    Task UpdateWorkoutNotesAsync(int workoutId, string? notes);
    Task UpdateExerciseNotesAsync(int workoutExerciseId, string? notes);
    Task<IReadOnlyList<WorkoutExercise>> GetWorkoutExercisesAsync(int workoutId);
    Task<IReadOnlyList<WorkoutSet>> GetSetsForExerciseAsync(int workoutExerciseId);
    Task<WorkoutSet?> GetLastSetForAutofillAsync(int exerciseId);
}
