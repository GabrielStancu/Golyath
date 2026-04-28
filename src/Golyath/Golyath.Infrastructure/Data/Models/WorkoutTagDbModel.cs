using SQLite;

namespace Golyath.Infrastructure.Data.Models;

[Table("WorkoutTags")]
internal class WorkoutTagDbModel
{
    [Indexed]
    public int WorkoutId { get; set; }

    [Indexed]
    public int TagId { get; set; }
}
