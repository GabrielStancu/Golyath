using Golyath.Application.DTOs;
using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Core.Enums;

namespace Golyath.Application.Services;

public sealed class RoutineService : IRoutineService
{
    private readonly IRoutineRepository _routines;
    private readonly IRoutineExerciseRepository _routineExercises;
    private readonly IExerciseRepository _exercises;
    private readonly IWorkoutRepository _workouts;

    private const int SecondsPerSet = 90;

    public RoutineService(
        IRoutineRepository routines,
        IRoutineExerciseRepository routineExercises,
        IExerciseRepository exercises,
        IWorkoutRepository workouts)
    {
        _routines = routines;
        _routineExercises = routineExercises;
        _exercises = exercises;
        _workouts = workouts;
    }

    public async Task<IReadOnlyList<RoutineSummaryDto>> GetAllRoutinesAsync()
    {
        var routines = await _routines.GetAllOrderedAsync();
        var summaries = new List<RoutineSummaryDto>();

        foreach (var r in routines)
            summaries.Add(await BuildSummaryAsync(r));

        return summaries;
    }

    public async Task<IReadOnlyList<RoutineSummaryDto>> GetTopRoutinesAsync(int count)
    {
        var all = await GetAllRoutinesAsync();
        return all.Take(count).ToList();
    }

    public async Task<RoutineDetailDto?> GetRoutineDetailAsync(int routineId)
    {
        var routine = await _routines.GetByIdAsync(routineId);
        if (routine is null) return null;

        var exercises = await _routineExercises.GetByRoutineIdAsync(routineId);
        var exerciseDtos = new List<RoutineExerciseDto>();

        foreach (var re in exercises)
        {
            var exercise = await _exercises.GetByIdAsync(re.ExerciseId);
            exerciseDtos.Add(new RoutineExerciseDto(
                re.Id,
                re.ExerciseId,
                exercise?.Name ?? "Unknown",
                re.Order,
                re.TargetSets,
                re.TargetReps,
                re.TargetWeight,
                re.RestSeconds));
        }

        return new RoutineDetailDto(routine.Id, routine.Name, routine.Category, exerciseDtos);
    }

    public async Task<Routine> CreateRoutineAsync(string name, RoutineCategory category)
    {
        var all = await _routines.GetAllOrderedAsync();
        var routine = new Routine
        {
            Name = name,
            Category = category,
            Order = all.Count,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _routines.InsertAsync(routine);
        return routine;
    }

    public async Task UpdateRoutineAsync(int routineId, string name, RoutineCategory category)
    {
        var routine = await _routines.GetByIdAsync(routineId);
        if (routine is null) return;

        routine.Name = name;
        routine.Category = category;
        routine.UpdatedAt = DateTime.UtcNow;
        await _routines.UpdateAsync(routine);
    }

    public async Task DeleteRoutineAsync(int routineId)
    {
        await _routineExercises.DeleteByRoutineIdAsync(routineId);
        await _routines.DeleteByIdAsync(routineId);
    }

    public async Task SetRoutineExercisesAsync(int routineId, IReadOnlyList<RoutineExerciseInput> exercises)
    {
        await _routineExercises.DeleteByRoutineIdAsync(routineId);

        foreach (var input in exercises)
        {
            var re = new RoutineExercise
            {
                RoutineId = routineId,
                ExerciseId = input.ExerciseId,
                Order = input.Order,
                TargetSets = input.TargetSets,
                TargetReps = input.TargetReps,
                TargetWeight = input.TargetWeight,
                RestSeconds = input.RestSeconds
            };
            await _routineExercises.InsertAsync(re);
        }

        var routine = await _routines.GetByIdAsync(routineId);
        if (routine is not null)
        {
            routine.UpdatedAt = DateTime.UtcNow;
            await _routines.UpdateAsync(routine);
        }
    }

    public async Task<RoutineSummaryDto?> GetNextRoutineInRotationAsync()
    {
        var routines = await _routines.GetAllOrderedAsync();
        if (routines.Count == 0) return null;

        // Find the most recent completed workout that has a RoutineId
        var recent = await _workouts.GetRecentAsync(50);
        var lastRoutineWorkout = recent
            .Where(w => w.CompletedAt.HasValue && w.RoutineId.HasValue)
            .OrderByDescending(w => w.CompletedAt)
            .FirstOrDefault();

        if (lastRoutineWorkout is null)
            return await BuildSummaryAsync(routines[0]);

        // Find the index of the last used routine and return the next one
        var lastIndex = routines.ToList().FindIndex(r => r.Id == lastRoutineWorkout.RoutineId);
        var nextIndex = (lastIndex + 1) % routines.Count;
        return await BuildSummaryAsync(routines[nextIndex]);
    }

    public async Task<int> GetEstimatedDurationAsync(int routineId)
    {
        // Try history-based first
        var recent = await _workouts.GetRecentAsync(50);
        var routineWorkouts = recent
            .Where(w => w.RoutineId == routineId && w.CompletedAt.HasValue && w.DurationSeconds > 0)
            .ToList();

        if (routineWorkouts.Count > 0)
        {
            var avgSeconds = routineWorkouts.Average(w => w.DurationSeconds);
            return (int)Math.Round(avgSeconds / 60.0);
        }

        // Fallback: totalSets × 90 seconds
        var exercises = await _routineExercises.GetByRoutineIdAsync(routineId);
        var totalSets = exercises.Sum(e => e.TargetSets);
        return Math.Max(1, (int)Math.Round(totalSets * SecondsPerSet / 60.0));
    }

    private async Task<RoutineSummaryDto> BuildSummaryAsync(Routine routine)
    {
        var exercises = await _routineExercises.GetByRoutineIdAsync(routine.Id);
        var totalSets = exercises.Sum(e => e.TargetSets);
        var duration = await GetEstimatedDurationAsync(routine.Id);

        return new RoutineSummaryDto(
            routine.Id,
            routine.Name,
            routine.Category,
            exercises.Count,
            totalSets,
            duration);
    }
}
