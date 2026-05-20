using Golyath.Core.Enums;
using SQLite;

namespace Golyath.Core.Entities;

[Table("Routines")]
public class Routine : BaseEntity
{
    [MaxLength(200), NotNull]
    public string Name { get; set; } = string.Empty;

    public RoutineCategory Category { get; set; } = RoutineCategory.Custom;

    public int Order { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
