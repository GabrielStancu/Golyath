namespace Golyath.Core.Entities;

public class WorkoutSet
{
    public int Id { get; set; }
    public int WorkoutExerciseId { get; set; }
    public int SetNumber { get; set; }
    public double? WeightKg { get; set; }
    public int? Reps { get; set; }
    public string? Tempo { get; set; }
    public string? Notes { get; set; }
    public bool IsPersonalRecord { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
