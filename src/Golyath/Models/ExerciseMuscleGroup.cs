using SQLite;

namespace Golyath.Models;

public enum MuscleRole
{
    Primary = 0,
    Secondary = 1,
    Stabilizer = 2
}

public class ExerciseMuscleGroup
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int ExerciseId { get; set; }

    [Indexed]
    public int MuscleGroupId { get; set; }

    public MuscleRole Role { get; set; }
}
