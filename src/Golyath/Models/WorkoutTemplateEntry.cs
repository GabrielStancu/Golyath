using SQLite;

namespace Golyath.Models;

public class WorkoutTemplateEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int TemplateId { get; set; }

    [Indexed]
    public int ExerciseId { get; set; }

    public int SortOrder { get; set; }

    public int TargetSets { get; set; } = 3;

    public int TargetReps { get; set; } = 10;

    public double TargetWeight { get; set; }

    /// <summary>Tempo string format "E-P1-C-P2", e.g. "3-1-2-0"</summary>
    public string TargetTempo { get; set; } = string.Empty;

    public int RestSeconds { get; set; } = 90;
}
