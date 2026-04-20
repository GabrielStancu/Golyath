using Golyath.Data;
using Golyath.Models;

namespace Golyath.Services;

public class WorkoutService : IWorkoutService
{
    private readonly GolyathDatabase _db;

    public WorkoutService(GolyathDatabase db)
    {
        _db = db;
    }

    public async Task<WorkoutSession> StartSessionAsync(int? templateId = null)
    {
        var session = new WorkoutSession
        {
            TemplateId = templateId,
            StartedAt = DateTime.UtcNow
        };
        await _db.InsertAsync(session);
        return session;
    }

    public async Task FinishSessionAsync(WorkoutSession session, string? notes = null, int? rating = null)
    {
        session.FinishedAt = DateTime.UtcNow;
        session.Notes = notes ?? session.Notes;
        session.Rating = rating ?? session.Rating;
        await _db.UpdateAsync(session);
    }

    public async Task<List<WorkoutSession>> GetSessionsAsync()
    {
        var conn = await _db.GetRawConnectionAsync();
        return await conn.QueryAsync<WorkoutSession>(
            "SELECT * FROM WorkoutSession WHERE FinishedAt IS NOT NULL ORDER BY StartedAt DESC");
    }

    public async Task<WorkoutSession?> GetSessionByIdAsync(int id)
    {
        return await _db.GetByIdAsync<WorkoutSession>(id);
    }

    public async Task<WorkoutSet> AddSetAsync(int sessionId, int exerciseId, int setNumber,
        double weight, int reps, bool isWarmup = false, string? tempo = null)
    {
        var set = new WorkoutSet
        {
            SessionId = sessionId,
            ExerciseId = exerciseId,
            SetNumber = setNumber,
            Weight = weight,
            Reps = reps,
            IsWarmup = isWarmup,
            Tempo = tempo ?? string.Empty,
            CompletedAt = DateTime.UtcNow
        };
        await _db.InsertAsync(set);
        return set;
    }

    public async Task UpdateSetAsync(WorkoutSet set)
    {
        await _db.UpdateAsync(set);
    }

    public async Task DeleteSetAsync(WorkoutSet set)
    {
        await _db.DeleteAsync(set);
    }

    public async Task<List<WorkoutSet>> GetSetsForSessionAsync(int sessionId)
    {
        var conn = await _db.GetRawConnectionAsync();
        return await conn.QueryAsync<WorkoutSet>(
            "SELECT * FROM WorkoutSet WHERE SessionId = ? ORDER BY ExerciseId ASC, SetNumber ASC",
            sessionId);
    }

    public async Task<List<WorkoutSet>> GetSetsForExerciseAsync(int exerciseId)
    {
        var conn = await _db.GetRawConnectionAsync();
        return await conn.QueryAsync<WorkoutSet>(
            "SELECT * FROM WorkoutSet WHERE ExerciseId = ? ORDER BY CompletedAt DESC",
            exerciseId);
    }

    public async Task<int> GetWorkoutsThisWeekAsync()
    {
        var monday = GetMondayOfCurrentWeek();
        var conn = await _db.GetRawConnectionAsync();
        var results = await conn.QueryAsync<WorkoutSession>(
            "SELECT * FROM WorkoutSession WHERE FinishedAt IS NOT NULL AND StartedAt >= ?",
            monday.ToString("yyyy-MM-dd HH:mm:ss"));
        return results.Count;
    }

    public async Task<double> GetTotalVolumeThisWeekAsync()
    {
        var monday = GetMondayOfCurrentWeek();
        var conn = await _db.GetRawConnectionAsync();

        // Get all sessions this week
        var sessions = await conn.QueryAsync<WorkoutSession>(
            "SELECT Id FROM WorkoutSession WHERE FinishedAt IS NOT NULL AND StartedAt >= ?",
            monday.ToString("yyyy-MM-dd HH:mm:ss"));

        if (sessions.Count == 0)
            return 0;

        double total = 0;
        foreach (var session in sessions)
        {
            var sets = await GetSetsForSessionAsync(session.Id);
            total += sets.Sum(s => s.Volume);
        }
        return total;
    }

    public async Task<WorkoutSession?> GetLastSessionAsync()
    {
        var conn = await _db.GetRawConnectionAsync();
        var results = await conn.QueryAsync<WorkoutSession>(
            "SELECT * FROM WorkoutSession WHERE FinishedAt IS NOT NULL ORDER BY StartedAt DESC LIMIT 1");
        return results.FirstOrDefault();
    }

    public async Task<List<string>> GetExerciseNamesForSessionAsync(int sessionId)
    {
        var conn = await _db.GetRawConnectionAsync();
        var sets = await conn.QueryAsync<WorkoutSet>(
            "SELECT DISTINCT ExerciseId FROM WorkoutSet WHERE SessionId = ?", sessionId);
        if (sets.Count == 0) return [];

        var idList = string.Join(",", sets.Select(s => s.ExerciseId).Distinct());
        var exercises = await conn.QueryAsync<Exercise>(
            $"SELECT * FROM Exercise WHERE Id IN ({idList})");
        return exercises.Select(e => e.Name).ToList();
    }

    private static DateTime GetMondayOfCurrentWeek()
    {
        var today = DateTime.UtcNow.Date;
        int daysFromMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return today.AddDays(-daysFromMonday);
    }
}
