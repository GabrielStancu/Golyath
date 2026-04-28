using SQLite;

namespace Golyath.Infrastructure.Data.Models;

[Table("WorkoutExercises")]
internal class WorkoutExerciseDbModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int WorkoutId { get; set; }
    public string ExerciseId { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Notes { get; set; }
}
