namespace Golyath.Core.Entities;

public class Exercise
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Force { get; set; }
    public string? Level { get; set; }
    public string? Mechanic { get; set; }
    public string? Equipment { get; set; }
    public List<string> PrimaryMuscles { get; set; } = [];
    public List<string> SecondaryMuscles { get; set; } = [];
    public List<string> Instructions { get; set; } = [];
    public string? Category { get; set; }
    public bool IsCustom { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
