using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Infrastructure.Database;

namespace Golyath.Infrastructure.Repositories;

public sealed class WorkoutTagRepository : IWorkoutTagRepository
{
    private readonly DatabaseService _databaseService;

    public WorkoutTagRepository(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<IReadOnlyList<Tag>> GetTagsForWorkoutAsync(int workoutId)
    {
        var db = await _databaseService.GetConnectionAsync();
        var links = await db.Table<WorkoutTag>()
            .Where(wt => wt.WorkoutId == workoutId)
            .ToListAsync();

        if (links.Count == 0)
            return [];

        var tagIds = links.Select(l => l.TagId).ToHashSet();
        var tags = await db.Table<Tag>().ToListAsync();
        return tags.Where(t => tagIds.Contains(t.Id)).ToList();
    }

    public async Task<IReadOnlyList<int>> GetWorkoutIdsForTagAsync(int tagId)
    {
        var db = await _databaseService.GetConnectionAsync();
        var links = await db.Table<WorkoutTag>()
            .Where(wt => wt.TagId == tagId)
            .ToListAsync();
        return links.Select(l => l.WorkoutId).ToList();
    }

    public async Task AddAsync(int workoutId, int tagId)
    {
        var db = await _databaseService.GetConnectionAsync();
        var existing = await db.Table<WorkoutTag>()
            .Where(wt => wt.WorkoutId == workoutId && wt.TagId == tagId)
            .FirstOrDefaultAsync();

        if (existing is null)
            await db.InsertAsync(new WorkoutTag { WorkoutId = workoutId, TagId = tagId });
    }

    public async Task RemoveAsync(int workoutId, int tagId)
    {
        var db = await _databaseService.GetConnectionAsync();
        await db.ExecuteAsync(
            "DELETE FROM WorkoutTags WHERE WorkoutId = ? AND TagId = ?",
            workoutId, tagId);
    }

    public async Task<IReadOnlyList<WorkoutTag>> GetAllAsync()
    {
        var db = await _databaseService.GetConnectionAsync();
        return await db.Table<WorkoutTag>().ToListAsync();
    }
}
