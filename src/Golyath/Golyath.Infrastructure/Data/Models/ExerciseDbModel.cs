using SQLite;

namespace Golyath.Infrastructure.Data.Models;

[Table("Exercises")]
internal class ExerciseDbModel
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Force { get; set; }
    public string? Level { get; set; }
    public string? Mechanic { get; set; }
    public string? Equipment { get; set; }
    public string PrimaryMusclesJson { get; set; } = "[]";
    public string SecondaryMusclesJson { get; set; } = "[]";
    public string InstructionsJson { get; set; } = "[]";
    public string? Category { get; set; }
    public bool IsCustom { get; set; }
    public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
}
