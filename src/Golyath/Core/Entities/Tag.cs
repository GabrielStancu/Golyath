using SQLite;

namespace Golyath.Core.Entities;

[Table("Tags")]
public class Tag : BaseEntity
{
    [MaxLength(100), NotNull, Unique]
    public string Name { get; set; } = string.Empty;

    [MaxLength(7)]
    public string? Color { get; set; }
}
