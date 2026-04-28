using SQLite;

namespace Golyath.Infrastructure.Data.Models;

[Table("WorkoutSets")]
internal class WorkoutSetDbModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int WorkoutExerciseId { get; set; }
    public int SetNumber { get; set; }
    public double? WeightKg { get; set; }
    public int? Reps { get; set; }
    public string? Tempo { get; set; }
    public string? Notes { get; set; }
    public bool IsPersonalRecord { get; set; }
    public string? CompletedAt { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
}
