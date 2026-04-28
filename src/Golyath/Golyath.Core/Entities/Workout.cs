namespace Golyath.Core.Entities;

public class Workout
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? Name { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public int? DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
