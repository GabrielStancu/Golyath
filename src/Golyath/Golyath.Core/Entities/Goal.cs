using Golyath.Core.Enums;

namespace Golyath.Core.Entities;

public class Goal
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public GoalType Type { get; set; }
    public string? ExerciseId { get; set; }
    public double TargetValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? AchievedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
