using Golyath.Core.Entities;

namespace Golyath.Application.Services;

public interface ITagService
{
    Task<IReadOnlyList<Tag>> GetAllTagsAsync();
    Task<Tag> GetOrCreateTagAsync(string name);
    Task<IReadOnlyList<Tag>> GetTagsForWorkoutAsync(int workoutId);
    Task AddTagToWorkoutAsync(int workoutId, int tagId);
    Task RemoveTagFromWorkoutAsync(int workoutId, int tagId);
}
