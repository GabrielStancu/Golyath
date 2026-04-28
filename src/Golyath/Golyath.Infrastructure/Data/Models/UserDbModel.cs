using SQLite;

namespace Golyath.Infrastructure.Data.Models;

[Table("Users")]
internal class UserDbModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string? Birthday { get; set; }
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }
    public int? Gender { get; set; }
    public int? FitnessGoal { get; set; }
    public int UnitSystem { get; set; }
    public bool OnboardingCompleted { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
    public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o");
}
