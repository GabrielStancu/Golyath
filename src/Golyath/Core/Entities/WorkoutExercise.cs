using SQLite;

namespace Golyath.Core.Entities;

[Table("WorkoutExercises")]
public class WorkoutExercise
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    [Indexed]
    public int WorkoutId { get; set; }
    public int ExerciseId { get; set; }
    public int OrderIndex { get; set; }
}
