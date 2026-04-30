using SQLite;

namespace Golyath.Core.Entities;

public abstract class BaseEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}
