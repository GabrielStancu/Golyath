using SQLite;
using Golyath.Core.Enums;

namespace Golyath.Core.Entities;

[Table("Exercises")]
public class Exercise
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public string Name { get; set; } = string.Empty;
    public string PrimaryMuscle { get; set; } = string.Empty;
    public string SecondaryMuscles { get; set; } = string.Empty; // JSON array stored as string
    public MovementType MovementType { get; set; }
    public EquipmentType EquipmentType { get; set; }
    public bool IsCustom { get; set; }
}
