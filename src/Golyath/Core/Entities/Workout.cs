using SQLite;

namespace Golyath.Core.Entities;

[Table("Workouts")]
public class Workout : BaseEntity
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [NotNull]
    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>Total elapsed duration in seconds.</summary>
    public int DurationSeconds { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
