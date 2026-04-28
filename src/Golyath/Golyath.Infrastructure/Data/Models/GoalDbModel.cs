using SQLite;

namespace Golyath.Infrastructure.Data.Models;

[Table("Goals")]
internal class GoalDbModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int Type { get; set; }
    public string? ExerciseId { get; set; }
    public double TargetValue { get; set; }
    public string StartDate { get; set; } = string.Empty;
    public string? TargetDate { get; set; }
    public string? AchievedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
}
