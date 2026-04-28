using Golyath.Core.Enums;

namespace Golyath.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public DateTime? Birthday { get; set; }
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }
    public Gender? Gender { get; set; }
    public FitnessGoal? FitnessGoal { get; set; }
    public UnitSystem UnitSystem { get; set; } = UnitSystem.Metric;
    public bool OnboardingCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
