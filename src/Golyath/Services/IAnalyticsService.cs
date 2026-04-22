namespace Golyath.Services;

public record MuscleVolume(string MuscleGroupName, double Volume, double Fraction);
public record WeeklyFrequency(string WeekLabel, int SessionCount);

public interface IAnalyticsService
{
    /// <summary>7 volume values (Mon–Sun) for the current week, in kg.</summary>
    Task<float[]> GetCurrentWeekVolumeAsync();

    /// <summary>Volume per week for the last <paramref name="weeks"/> weeks, oldest first.</summary>
    Task<List<(string Label, float Volume)>> GetWeeklyVolumeHistoryAsync(int weeks = 8);

    /// <summary>Muscle group volumes for the last <paramref name="days"/> days, sorted by volume desc.</summary>
    Task<List<MuscleVolume>> GetMuscleDistributionAsync(int days = 30);

    /// <summary>Session count per week for the last <paramref name="weeks"/> weeks.</summary>
    Task<List<WeeklyFrequency>> GetWorkoutFrequencyAsync(int weeks = 8);
}
