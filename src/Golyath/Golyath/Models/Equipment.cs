using SQLite;

namespace Golyath.Models;

public class Equipment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string Name { get; set; } = string.Empty;
}
