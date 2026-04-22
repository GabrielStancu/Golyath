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

    /// <summary>Exercise folder name under Resources/Raw/exercises/ (e.g. "Barbell_Bench_Press_-_Medium_Grip")</summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>Derived path to frame 0 image. Not stored in DB.</summary>
    [Ignore]
    public string ImageFrame0 => string.IsNullOrEmpty(ImagePath) ? string.Empty : $"exercises/{ImagePath}/0.jpg";

    /// <summary>Derived path to frame 1 image. Not stored in DB.</summary>
    [Ignore]
    public string ImageFrame1 => string.IsNullOrEmpty(ImagePath) ? string.Empty : $"exercises/{ImagePath}/1.jpg";

    public bool IsCustom { get; set; }

    public bool IsCompound { get; set; }

    /// <summary>Populated at runtime from the MuscleGroup table. Not stored in DB.</summary>
    [Ignore]
    public string MuscleGroupName { get; set; } = string.Empty;
}
