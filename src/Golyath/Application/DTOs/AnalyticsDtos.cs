namespace Golyath.Application.DTOs;

/// <summary>A single (date, max-weight) data point for an exercise's strength progression.</summary>
public record StrengthPoint(DateTime Date, double MaxWeight);

/// <summary>Strength-progression data for one exercise over a chosen period.</summary>
public record StrengthProgressionData(string ExerciseName, IReadOnlyList<StrengthPoint> Points);

/// <summary>Total training volume (weight × reps) for one calendar week.</summary>
public record VolumePoint(string Label, double Volume);

/// <summary>Completed-set count and relative fraction for one muscle group.</summary>
public record MuscleGroupVolume(string MuscleGroup, int SetCount, double Fraction);

/// <summary>Lightweight exercise reference used in the analytics exercise picker.</summary>
public record ExerciseOption(int Id, string Name);

/// <summary>One of the 5 fixed muscle balance groups with its relative fraction (0–1, where 1 = most trained).</summary>
public record MuscleBalanceItem(string Label, double Fraction);
