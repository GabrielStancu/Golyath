using Golyath.Models;

namespace Golyath.Services;

public interface IWorkoutService
{
    // Sessions
    Task<WorkoutSession> StartSessionAsync(int? templateId = null);
    Task FinishSessionAsync(WorkoutSession session, string? notes = null, int? rating = null);
    Task<List<WorkoutSession>> GetSessionsAsync();
    Task<WorkoutSession?> GetSessionByIdAsync(int id);

    // Sets
    Task<WorkoutSet> AddSetAsync(int sessionId, int exerciseId, int setNumber,
        double weight, int reps, bool isWarmup = false, string? tempo = null);
    Task UpdateSetAsync(WorkoutSet set);
    Task DeleteSetAsync(WorkoutSet set);
    Task<List<WorkoutSet>> GetSetsForSessionAsync(int sessionId);
    Task<List<WorkoutSet>> GetSetsForExerciseAsync(int exerciseId);

    // Stats
    Task<int> GetWorkoutsThisWeekAsync();
    Task<double> GetTotalVolumeThisWeekAsync();
    Task<WorkoutSession?> GetLastSessionAsync();
    Task<List<string>> GetExerciseNamesForSessionAsync(int sessionId);
    Task<List<WorkoutSession>> GetRecentSessionsAsync(int count);
    Task<double> GetAvgDurationMinutesAsync(int days);
}
