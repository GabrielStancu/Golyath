using SQLite;

namespace Golyath.Models;

public class WorkoutTemplate
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUsedAt { get; set; }

    /// <summary>Hex accent color for the template card, e.g. "#F5C518"</summary>
    public string Color { get; set; } = "#F5C518";
}
