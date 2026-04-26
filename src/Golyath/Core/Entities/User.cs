using SQLite;
using Golyath.Core.Enums;

namespace Golyath.Core.Entities;

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public DateTime Birthday { get; set; }
    public double Height { get; set; }   // cm
    public double Weight { get; set; }   // stored in kg
    public Gender Gender { get; set; }
    public FitnessGoal FitnessGoal { get; set; }
    public DateTime CreatedAt { get; set; }
}
