using Golyath.Application.DTOs;
using Golyath.Core.Entities;

namespace Golyath.Application.Services;

public interface IWorkoutHistoryService
{
    /// <summary>
    /// Returns completed workouts, newest first.
    /// Pass <paramref name="tagId"/> to filter by a specific tag.
    /// Pass <paramref name="from"/> / <paramref name="to"/> to restrict the date range.
    /// </summary>
    Task<IReadOnlyList<WorkoutHistorySummaryDto>> GetHistoryAsync(
        DateTime? from = null,
        DateTime? to = null,
        int? tagId = null);

    /// <summary>Full detail for a single completed workout.</summary>
    Task<WorkoutHistoryDetailDto?> GetWorkoutDetailAsync(int workoutId);

    /// <summary>Tags currently attached to a workout.</summary>
    Task<IReadOnlyList<Tag>> GetTagsForWorkoutAsync(int workoutId);

    /// <summary>All tags that have been created.</summary>
    Task<IReadOnlyList<Tag>> GetAllTagsAsync();

    /// <summary>Returns the tag with that name, creating it if it doesn't exist.</summary>
    Task<Tag> GetOrCreateTagAsync(string name);

    /// <summary>Attaches a tag to a workout.</summary>
    Task AssignTagAsync(int workoutId, int tagId);

    /// <summary>Removes a tag from a workout.</summary>
    Task RemoveTagAsync(int workoutId, int tagId);
}
