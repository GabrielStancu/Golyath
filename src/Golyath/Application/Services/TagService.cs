using Golyath.Core.Abstractions;
using Golyath.Core.Entities;

namespace Golyath.Application.Services;

public sealed class TagService : ITagService
{
    private readonly ITagRepository _tagRepository;
    private readonly IWorkoutTagRepository _workoutTagRepository;

    public TagService(ITagRepository tagRepository, IWorkoutTagRepository workoutTagRepository)
    {
        _tagRepository = tagRepository;
        _workoutTagRepository = workoutTagRepository;
    }

    public Task<IReadOnlyList<Tag>> GetAllTagsAsync() =>
        _tagRepository.GetAllAsync();

    public async Task<Tag> GetOrCreateTagAsync(string name)
    {
        name = name.Trim();
        var existing = await _tagRepository.GetByNameAsync(name);
        if (existing is not null)
            return existing;

        var tag = new Tag { Name = name };
        await _tagRepository.InsertAsync(tag);
        return tag;
    }

    public Task<IReadOnlyList<Tag>> GetTagsForWorkoutAsync(int workoutId) =>
        _workoutTagRepository.GetTagsForWorkoutAsync(workoutId);

    public Task AddTagToWorkoutAsync(int workoutId, int tagId) =>
        _workoutTagRepository.AddAsync(workoutId, tagId);

    public Task RemoveTagFromWorkoutAsync(int workoutId, int tagId) =>
        _workoutTagRepository.RemoveAsync(workoutId, tagId);
}
