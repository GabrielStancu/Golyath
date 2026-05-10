namespace Golyath.Application.DTOs;

/// <summary>
/// Best-ever performance achieved by a user for a specific exercise.
/// All weight values are in the user's stored unit (kg or lb — not converted here).
/// </summary>
public record PersonalRecord(
    int ExerciseId,
    string ExerciseName,
    /// <summary>Heaviest weight lifted in a single completed set.</summary>
    double MaxWeight,
    /// <summary>Most reps in a single completed set (any weight).</summary>
    int MaxReps,
    /// <summary>Highest single-set volume (weight × reps).</summary>
    double MaxVolume,
    /// <summary>
    /// Estimated one-rep max using the Epley formula: weight × (1 + reps / 30).
    /// Returns 0 when no completed sets exist.
    /// </summary>
    double EstimatedOneRM,
    /// <summary>UTC timestamp of the set that produced the highest estimated 1RM.</summary>
    DateTime AchievedAt);
