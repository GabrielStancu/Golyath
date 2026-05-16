namespace Golyath.Application.Services;

public interface IExerciseService
{
    /// <summary>
    /// Deletes a custom exercise and all associated WorkoutExercise/WorkoutSet records.
    /// No-op if the exercise is not custom.
    /// </summary>
    Task DeleteCustomExerciseAsync(int exerciseId);

    /// <summary>
    /// Returns the count of past workouts that include this exercise.
    /// Used to surface a warning before deletion.
    /// </summary>
    Task<int> GetWorkoutUsageCountAsync(int exerciseId);
}
