using SQLite;

namespace Golyath.Models;

public class WorkoutSet
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int SessionId { get; set; }

    [Indexed]
    public int ExerciseId { get; set; }

    public int SetNumber { get; set; }

    public int Reps { get; set; }

    /// <summary>Weight in kilograms. Convert to lbs for display when user prefers imperial.</summary>
    public double Weight { get; set; }

    /// <summary>Optional tempo string "E-P1-C-P2", e.g. "3-1-2-0". Empty when not tracked.</summary>
    public string Tempo { get; set; } = string.Empty;

    public bool IsWarmup { get; set; }

    /// <summary>Rate of Perceived Exertion 1–10. Null when not provided.</summary>
    public int? RPE { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    [Ignore]
    public double Volume => Reps * Weight;

    /// <summary>Epley estimated 1-Rep Max: weight × (1 + reps / 30)</summary>
    [Ignore]
    public double Estimated1RM => Reps > 0 ? Weight * (1 + Reps / 30.0) : Weight;
}
