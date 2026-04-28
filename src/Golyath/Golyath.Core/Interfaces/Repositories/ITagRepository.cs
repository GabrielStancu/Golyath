using Golyath.Core.Entities;

namespace Golyath.Core.Interfaces.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(int id);
    Task<IEnumerable<Tag>> GetAllAsync();
    Task<IEnumerable<Tag>> GetByWorkoutIdAsync(int workoutId);
    Task<int> AddAsync(Tag tag);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(int id);
    Task AddWorkoutTagAsync(int workoutId, int tagId);
    Task RemoveWorkoutTagAsync(int workoutId, int tagId);
}
