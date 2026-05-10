using Golyath.Core.Enums;

namespace Golyath.Application.DTOs;

/// <summary>Root document for Golyath JSON backup files.</summary>
public class BackupDocument
{
    public int BackupSchemaVersion { get; set; } = 1;
    public string AppVersion { get; set; } = "1.0.0";
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    public BackupData Data { get; set; } = new();
}

public class BackupData
{
    public List<UserBackup> Users { get; set; } = [];
    /// <summary>ALL exercises (both seeded and custom). IsCustom flag distinguishes them. Needed for workout ID remapping on import.</summary>
    public List<ExerciseBackup> Exercises { get; set; } = [];
    public List<WorkoutBackup> Workouts { get; set; } = [];
    public List<WorkoutExerciseBackup> WorkoutExercises { get; set; } = [];
    public List<WorkoutSetBackup> WorkoutSets { get; set; } = [];
    public List<GoalBackup> Goals { get; set; } = [];
    public List<TagBackup> Tags { get; set; } = [];
    public List<WorkoutTagBackup> WorkoutTags { get; set; } = [];
}

public record UserBackup(int Id, string Nickname, DateTime Birthday, double HeightCm, double WeightKg, Gender Gender, FitnessGoal FitnessGoal, WeightUnit PreferredUnit, DateTime CreatedAt, DateTime UpdatedAt);
public record ExerciseBackup(int Id, string Name, bool IsCustom, MuscleGroup PrimaryMuscle, string SecondaryMusclesRaw, MovementType MovementType, EquipmentType Equipment, string? Notes, string? ExternalId, string InstructionsRaw);
public record WorkoutBackup(int Id, string? Name, DateTime StartedAt, DateTime? CompletedAt, int DurationSeconds, string? Notes, DateTime CreatedAt);
public record WorkoutExerciseBackup(int Id, int WorkoutId, int ExerciseId, int Order, string? Notes);
public record WorkoutSetBackup(int Id, int WorkoutExerciseId, int SetNumber, double Weight, int Reps, string? Tempo, string? Notes, bool IsCompleted, DateTime? CompletedAt);
public record GoalBackup(int Id, int UserId, GoalType Type, string Description, double TargetValue, double CurrentValue, int? ExerciseId, DateTime? TargetDate, bool IsCompleted, DateTime CreatedAt, DateTime UpdatedAt);
public record TagBackup(int Id, string Name, string? Color);
public record WorkoutTagBackup(int Id, int WorkoutId, int TagId);
