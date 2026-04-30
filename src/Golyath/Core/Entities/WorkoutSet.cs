using SQLite;

namespace Golyath.Core.Entities;

[Table("WorkoutSets")]
public class WorkoutSet : BaseEntity
{
    [Indexed, NotNull]
    public int WorkoutExerciseId { get; set; }

    public int SetNumber { get; set; }

    public double Weight { get; set; }

    public int Reps { get; set; }

    /// <summary>Tempo notation, e.g. "3-1-2-0" (eccentric-pause-concentric-pause).</summary>
    [MaxLength(20)]
    public string? Tempo { get; set; }

    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }
}
