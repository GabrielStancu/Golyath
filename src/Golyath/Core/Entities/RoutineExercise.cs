using SQLite;

namespace Golyath.Core.Entities;

[Table("RoutineExercises")]
public class RoutineExercise : BaseEntity
{
    [Indexed, NotNull]
    public int RoutineId { get; set; }

    [Indexed, NotNull]
    public int ExerciseId { get; set; }

    public int Order { get; set; }

    public int TargetSets { get; set; } = 3;

    public int TargetReps { get; set; } = 10;

    public double? TargetWeight { get; set; }

    public int RestSeconds { get; set; } = 90;
}
