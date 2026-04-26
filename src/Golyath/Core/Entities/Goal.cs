using SQLite;
using Golyath.Core.Enums;

namespace Golyath.Core.Entities;

[Table("Goals")]
public class Goal
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int UserId { get; set; }
    public GoalType Type { get; set; }
    public double TargetValue { get; set; }
    public DateTime CreatedAt { get; set; }
}
