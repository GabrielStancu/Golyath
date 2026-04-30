using SQLite;

namespace Golyath.Core.Entities;

/// <summary>Join table linking workouts to tags.</summary>
[Table("WorkoutTags")]
public class WorkoutTag
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed, NotNull]
    public int WorkoutId { get; set; }

    [Indexed, NotNull]
    public int TagId { get; set; }
}
