using SQLite;

namespace Golyath.Models;

public class WorkoutSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>Null when the session is freeform (not from a template).</summary>
    public int? TemplateId { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? FinishedAt { get; set; }

    public string Notes { get; set; } = string.Empty;

    /// <summary>User rating 1–5. Null if not rated.</summary>
    public int? Rating { get; set; }

    [Ignore]
    public TimeSpan Duration =>
        FinishedAt.HasValue ? FinishedAt.Value - StartedAt : TimeSpan.Zero;
}
