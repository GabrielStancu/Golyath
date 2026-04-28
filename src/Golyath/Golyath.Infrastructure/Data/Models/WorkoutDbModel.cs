using SQLite;

namespace Golyath.Infrastructure.Data.Models;

[Table("Workouts")]
internal class WorkoutDbModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? Name { get; set; }
    public string StartedAt { get; set; } = string.Empty;
    public string? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public int? DurationSeconds { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
}
