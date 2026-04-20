using SQLite;

namespace Golyath.Models;

public class Exercise
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Indexed]
    public int PrimaryMuscleGroupId { get; set; }

    public int EquipmentId { get; set; }

    /// <summary>Relative path under Resources/Raw/exercises/</summary>
    public string ImagePath { get; set; } = string.Empty;

    public bool IsCustom { get; set; }

    public bool IsCompound { get; set; }

    /// <summary>Populated at runtime from the MuscleGroup table. Not stored in DB.</summary>
    [Ignore]
    public string MuscleGroupName { get; set; } = string.Empty;
}
