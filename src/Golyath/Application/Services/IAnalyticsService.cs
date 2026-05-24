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

    /// <summary>
    /// Returns the 5 fixed muscle balance groups (Chest, Back, Legs, Shoulders, Core).
    /// Fraction is relative to the most-trained group (max = 1.0).
    /// </summary>
    Task<IReadOnlyList<MuscleBalanceItem>> GetMuscleBalanceAsync(DateTime from);

    /// <summary>Recovery score 0–100 derived from consecutive training days and rest since last session.</summary>
    Task<int> GetRecoveryScoreAsync();

    /// <summary>Intensity score 0–100 derived from avg sets/session and sessions/week in the given period.</summary>
    Task<int> GetIntensityScoreAsync(DateTime from);
}
