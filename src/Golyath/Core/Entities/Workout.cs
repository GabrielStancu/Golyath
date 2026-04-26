using SQLite;

namespace Golyath.Core.Entities;

[Table("Workouts")]
public class Workout
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty; // JSON array stored as string
}
