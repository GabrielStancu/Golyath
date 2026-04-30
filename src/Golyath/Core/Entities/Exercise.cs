using Golyath.Core.Enums;
using SQLite;

namespace Golyath.Core.Entities;

[Table("Exercises")]
public class Exercise : BaseEntity
{
    [MaxLength(200), NotNull]
    public string Name { get; set; } = string.Empty;

    public MuscleGroup PrimaryMuscle { get; set; }

    /// <summary>
    /// Comma-separated MuscleGroup enum names. Use SecondaryMuscles for typed access.
    /// </summary>
    public string SecondaryMusclesRaw { get; set; } = string.Empty;

    [Ignore]
    public IList<MuscleGroup> SecondaryMuscles
    {
        get => string.IsNullOrWhiteSpace(SecondaryMusclesRaw)
            ? []
            : SecondaryMusclesRaw
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Enum.Parse<MuscleGroup>(s))
                .ToList();
        set => SecondaryMusclesRaw = string.Join(',', value.Select(m => m.ToString()));
    }

    public MovementType MovementType { get; set; }

    public EquipmentType Equipment { get; set; }

    public bool IsCustom { get; set; }

    public string? Notes { get; set; }
}
