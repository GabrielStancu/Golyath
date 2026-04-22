using Golyath.Data;
using Golyath.Models;

namespace Golyath.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly GolyathDatabase _db;

    public AnalyticsService(GolyathDatabase db) => _db = db;

    public async Task<float[]> GetCurrentWeekVolumeAsync()
    {
        var monday = GetMondayOfWeek(DateTime.UtcNow);
        var sunday = monday.AddDays(7);
        var conn = await _db.GetRawConnectionAsync();

        var sessions = await conn.QueryAsync<WorkoutSession>(
            "SELECT * FROM WorkoutSession WHERE FinishedAt IS NOT NULL AND StartedAt >= ? AND StartedAt < ?",
            monday.ToString("yyyy-MM-dd HH:mm:ss"), sunday.ToString("yyyy-MM-dd HH:mm:ss"));

        var volumes = new float[7]; // Mon=0 … Sun=6
        foreach (var session in sessions)
        {
            int dayIdx = ((int)session.StartedAt.ToLocalTime().DayOfWeek + 6) % 7;
            var sets = await conn.QueryAsync<WorkoutSet>(
                "SELECT * FROM WorkoutSet WHERE SessionId = ?", session.Id);
            volumes[dayIdx] += (float)sets.Sum(s => s.Volume);
        }
        return volumes;
    }

    public async Task<List<(string Label, float Volume)>> GetWeeklyVolumeHistoryAsync(int weeks = 8)
    {
        var thisMonday = GetMondayOfWeek(DateTime.UtcNow);
        var cutoff = thisMonday.AddDays(-(weeks - 1) * 7);
        var conn = await _db.GetRawConnectionAsync();

        var result = new List<(string Label, float Volume)>();
        for (int w = 0; w < weeks; w++)
        {
            var weekStart = cutoff.AddDays(w * 7);
            var weekEnd = weekStart.AddDays(7);

            var sessions = await conn.QueryAsync<WorkoutSession>(
                "SELECT * FROM WorkoutSession WHERE FinishedAt IS NOT NULL AND StartedAt >= ? AND StartedAt < ?",
                weekStart.ToString("yyyy-MM-dd"), weekEnd.ToString("yyyy-MM-dd"));

            float vol = 0;
            foreach (var s in sessions)
            {
                var sets = await conn.QueryAsync<WorkoutSet>(
                    "SELECT * FROM WorkoutSet WHERE SessionId = ?", s.Id);
                vol += (float)sets.Sum(x => x.Volume);
            }

            string label = w == weeks - 1 ? "This" : weekStart.ToString("MMM d");
            result.Add((label, vol));
        }
        return result;
    }

    public async Task<List<MuscleVolume>> GetMuscleDistributionAsync(int days = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);
        var conn = await _db.GetRawConnectionAsync();

        var sessions = await conn.QueryAsync<WorkoutSession>(
            "SELECT * FROM WorkoutSession WHERE FinishedAt IS NOT NULL AND StartedAt >= ?",
            cutoff.ToString("yyyy-MM-dd HH:mm:ss"));

        var muscleVolumes = new Dictionary<int, double>();

        foreach (var session in sessions)
        {
            var sets = await conn.QueryAsync<WorkoutSet>(
                "SELECT * FROM WorkoutSet WHERE SessionId = ?", session.Id);

            foreach (var set in sets)
            {
                var exercise = await _db.GetByIdAsync<Exercise>(set.ExerciseId);
                if (exercise is null) continue;

                // Primary muscle contributes 100 %
                muscleVolumes.TryGetValue(exercise.PrimaryMuscleGroupId, out double pv);
                muscleVolumes[exercise.PrimaryMuscleGroupId] = pv + set.Volume;

                // Secondary / stabilizer muscles
                var links = await conn.QueryAsync<ExerciseMuscleGroup>(
                    "SELECT * FROM ExerciseMuscleGroup WHERE ExerciseId = ?", set.ExerciseId);
                foreach (var link in links)
                {
                    double factor = link.Role == MuscleRole.Secondary ? 0.5 : 0.25;
                    muscleVolumes.TryGetValue(link.MuscleGroupId, out double sv);
                    muscleVolumes[link.MuscleGroupId] = sv + set.Volume * factor;
                }
            }
        }

        if (muscleVolumes.Count == 0) return [];

        double total = muscleVolumes.Values.Sum();
        var muscleGroups = await _db.GetAllAsync<MuscleGroup>();
        var nameMap = muscleGroups.ToDictionary(m => m.Id, m => m.DisplayName);

        return muscleVolumes
            .Where(kvp => kvp.Value > 0)
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => new MuscleVolume(
                nameMap.GetValueOrDefault(kvp.Key, $"Group {kvp.Key}"),
                kvp.Value,
                total > 0 ? kvp.Value / total : 0))
            .ToList();
    }

    public async Task<List<WeeklyFrequency>> GetWorkoutFrequencyAsync(int weeks = 8)
    {
        var thisMonday = GetMondayOfWeek(DateTime.UtcNow);
        var cutoff = thisMonday.AddDays(-(weeks - 1) * 7);
        var conn = await _db.GetRawConnectionAsync();

        var result = new List<WeeklyFrequency>();
        for (int w = 0; w < weeks; w++)
        {
            var weekStart = cutoff.AddDays(w * 7);
            var weekEnd = weekStart.AddDays(7);

            var sessions = await conn.QueryAsync<WorkoutSession>(
                "SELECT * FROM WorkoutSession WHERE FinishedAt IS NOT NULL AND StartedAt >= ? AND StartedAt < ?",
                weekStart.ToString("yyyy-MM-dd"), weekEnd.ToString("yyyy-MM-dd"));

            string label = w == weeks - 1 ? "This" : weekStart.ToString("M/d");
            result.Add(new WeeklyFrequency(label, sessions.Count));
        }
        return result;
    }

    private static DateTime GetMondayOfWeek(DateTime date)
    {
        var d = date.Date;
        int daysFromMonday = ((int)d.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return d.AddDays(-daysFromMonday);
    }
}
