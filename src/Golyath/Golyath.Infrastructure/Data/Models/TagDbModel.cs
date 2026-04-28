using SQLite;

namespace Golyath.Infrastructure.Data.Models;

[Table("Tags")]
internal class TagDbModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }
}
