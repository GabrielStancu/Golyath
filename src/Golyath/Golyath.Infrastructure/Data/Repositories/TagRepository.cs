using Golyath.Core.Entities;
using Golyath.Core.Interfaces.Repositories;
using Golyath.Infrastructure.Data;
using Golyath.Infrastructure.Data.Models;

namespace Golyath.Infrastructure.Data.Repositories;

internal class TagRepository : ITagRepository
{
    private readonly AppDatabase _database;

    public TagRepository(AppDatabase database)
    {
        _database = database;
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        var model = await _database.Connection.FindAsync<TagDbModel>(id);
        return model is null ? null : MapToEntity(model);
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        var models = await _database.Connection.Table<TagDbModel>().ToListAsync();
        return models.Select(MapToEntity);
    }

    public async Task<IEnumerable<Tag>> GetByWorkoutIdAsync(int workoutId)
    {
        var models = await _database.Connection.QueryAsync<TagDbModel>(@"
            SELECT t.* FROM Tags t
            INNER JOIN WorkoutTags wt ON t.Id = wt.TagId
            WHERE wt.WorkoutId = ?", workoutId);
        return models.Select(MapToEntity);
    }

    public async Task<int> AddAsync(Tag tag)
    {
        var model = MapToModel(tag);
        await _database.Connection.InsertAsync(model);
        return model.Id;
    }

    public async Task UpdateAsync(Tag tag)
    {
        await _database.Connection.UpdateAsync(MapToModel(tag));
    }

    public async Task DeleteAsync(int id)
    {
        await _database.Connection.DeleteAsync<TagDbModel>(id);
    }

    public async Task AddWorkoutTagAsync(int workoutId, int tagId)
    {
        await _database.Connection.InsertAsync(new WorkoutTagDbModel
        {
            WorkoutId = workoutId,
            TagId = tagId,
        });
    }

    public async Task RemoveWorkoutTagAsync(int workoutId, int tagId)
    {
        await _database.Connection.ExecuteAsync(
            "DELETE FROM WorkoutTags WHERE WorkoutId = ? AND TagId = ?",
            workoutId, tagId);
    }

    private static Tag MapToEntity(TagDbModel m) => new()
    {
        Id = m.Id,
        Name = m.Name,
        Color = m.Color,
    };

    private static TagDbModel MapToModel(Tag e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Color = e.Color,
    };
}
