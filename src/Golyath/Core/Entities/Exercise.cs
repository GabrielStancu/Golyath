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

    /// <summary>Original dataset ID (e.g. "Barbell_Squat"). Null for custom exercises.</summary>
    public string? ExternalId { get; set; }

    /// <summary>Newline-separated step-by-step instructions. Use Instructions for typed access.</summary>
    public string InstructionsRaw { get; set; } = string.Empty;

    [Ignore]
    public IList<string> Instructions
    {
        get => string.IsNullOrWhiteSpace(InstructionsRaw)
            ? []
            : InstructionsRaw.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        set => InstructionsRaw = string.Join('\n', value);
    }

    /// <summary>Virtual path to the first exercise image in the raw asset bundle. Null for custom exercises without images.</summary>
    [Ignore]
    public string? PrimaryImagePath => ExternalId is null ? null : $"exercises/{ExternalId}/0.jpg";

    /// <summary>Secondary muscles formatted for display (e.g. "Forearms, Biceps"). Empty string if none.</summary>
    [Ignore]
    public string SecondaryMusclesDisplay =>
        string.IsNullOrWhiteSpace(SecondaryMusclesRaw)
            ? string.Empty
            : string.Join(", ", SecondaryMusclesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries));
}
