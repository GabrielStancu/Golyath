namespace Golyath.Core.Entities;

public class WorkoutExercise
{
    public int Id { get; set; }
    public int WorkoutId { get; set; }
    public string ExerciseId { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Notes { get; set; }
}
