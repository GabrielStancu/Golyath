using SQLite;

namespace Golyath.Core.Entities;

[Table("Tags")]
public class Tag
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
