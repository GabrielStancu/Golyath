using SQLite;

namespace Golyath.Core.Entities;

[Table("Sets")]
public class Set
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public int WorkoutExerciseId { get; set; }
    public double Weight { get; set; }  // kg
    public int Reps { get; set; }
    public string Tempo { get; set; } = string.Empty;  // e.g. "3-1-2"
    public string Notes { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}
