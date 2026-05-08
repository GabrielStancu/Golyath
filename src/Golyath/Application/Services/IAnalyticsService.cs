using Golyath.Application.DTOs;

namespace Golyath.Application.Services;

public interface IAnalyticsService
{
    /// <summary>Returns exercises that have at least one logged set.</summary>
    Task<IReadOnlyList<ExerciseOption>> GetExercisesWithHistoryAsync();

    /// <summary>Max weight per session for the given exercise, ordered by date.</summary>
    Task<StrengthProgressionData?> GetStrengthProgressionAsync(int exerciseId, DateTime from);

    /// <summary>Total training volume (weight × reps) grouped by calendar week, oldest first.</summary>
    Task<IReadOnlyList<VolumePoint>> GetWeeklyVolumeAsync(DateTime from);

    /// <summary>Completed-set counts per primary muscle group, ordered by frequency descending.</summary>
    Task<IReadOnlyList<MuscleGroupVolume>> GetMuscleGroupDistributionAsync(DateTime from);
}
