using Golyath.Core.Abstractions;

namespace Golyath.Application.Services;

public sealed class ExerciseService : IExerciseService
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IWorkoutExerciseRepository _workoutExerciseRepository;
    private readonly IWorkoutSetRepository _workoutSetRepository;

    public ExerciseService(
        IExerciseRepository exerciseRepository,
        IWorkoutExerciseRepository workoutExerciseRepository,
        IWorkoutSetRepository workoutSetRepository)
    {
        _exerciseRepository = exerciseRepository;
        _workoutExerciseRepository = workoutExerciseRepository;
        _workoutSetRepository = workoutSetRepository;
    }

    public async Task DeleteCustomExerciseAsync(int exerciseId)
    {
        var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);
        if (exercise is null || !exercise.IsCustom)
            return;

        // Find all WorkoutExercise records that reference this exercise
        var workoutExercises = await _workoutExerciseRepository.GetByExerciseIdAsync(exerciseId);

        // Delete children first: WorkoutSets → WorkoutExercises
        foreach (var we in workoutExercises)
        {
            var sets = await _workoutSetRepository.GetByWorkoutExerciseIdAsync(we.Id);
            foreach (var set in sets)
                await _workoutSetRepository.DeleteAsync(set);

            await _workoutExerciseRepository.DeleteAsync(we);
        }

        // Delete the exercise itself
        await _exerciseRepository.DeleteAsync(exercise);
    }

    public async Task<int> GetWorkoutUsageCountAsync(int exerciseId)
    {
        var workoutExercises = await _workoutExerciseRepository.GetByExerciseIdAsync(exerciseId);
        return workoutExercises.Select(we => we.WorkoutId).Distinct().Count();
    }
}
