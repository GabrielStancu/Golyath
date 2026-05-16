using Golyath.Core.Entities;

namespace Golyath.Core.Abstractions;

public interface IWorkoutTagRepository
{
    Task<IReadOnlyList<Tag>> GetTagsForWorkoutAsync(int workoutId);
    Task<IReadOnlyList<int>> GetWorkoutIdsForTagAsync(int tagId);
    Task AddAsync(int workoutId, int tagId);
    Task RemoveAsync(int workoutId, int tagId);
    Task RemoveAllForWorkoutAsync(int workoutId);
    /// <summary>Returns all workout-tag links. Used for export.</summary>
    Task<IReadOnlyList<WorkoutTag>> GetAllAsync();
}
