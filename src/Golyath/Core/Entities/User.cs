using Golyath.Core.Enums;
using SQLite;

namespace Golyath.Core.Entities;

[Table("Users")]
public class User : BaseEntity
{
    [MaxLength(100), NotNull]
    public string Nickname { get; set; } = string.Empty;

    public DateTime Birthday { get; set; }

    public double HeightCm { get; set; }

    public double WeightKg { get; set; }

    public Gender Gender { get; set; }

    public FitnessGoal FitnessGoal { get; set; }

    public WeightUnit PreferredUnit { get; set; } = WeightUnit.Kg;

    public AppLanguage Language { get; set; } = AppLanguage.English;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
