using SQLite;

namespace Golyath.Core.Entities;

[Table("WorkoutExercises")]
public class WorkoutExercise : BaseEntity
{
    [Indexed, NotNull]
    public int WorkoutId { get; set; }

    [Indexed, NotNull]
    public int ExerciseId { get; set; }

    public int Order { get; set; }

    public string? Notes { get; set; }
}
