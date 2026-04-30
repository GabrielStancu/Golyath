using Golyath.Core.Enums;
using SQLite;

namespace Golyath.Core.Entities;

[Table("Goals")]
public class Goal : BaseEntity
{
    [Indexed, NotNull]
    public int UserId { get; set; }

    public GoalType Type { get; set; }

    [MaxLength(500), NotNull]
    public string Description { get; set; } = string.Empty;

    public double TargetValue { get; set; }

    public double CurrentValue { get; set; }

    /// <summary>Optional — links a strength goal to a specific exercise.</summary>
    public int? ExerciseId { get; set; }

    public DateTime? TargetDate { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
