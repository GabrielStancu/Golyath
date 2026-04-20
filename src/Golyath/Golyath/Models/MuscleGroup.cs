using SQLite;

namespace Golyath.Models;

public class MuscleGroup
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Upper, Lower, Core</summary>
    public string BodyRegion { get; set; } = string.Empty;

    public string IconPath { get; set; } = string.Empty;
}
