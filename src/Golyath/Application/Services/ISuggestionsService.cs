using Golyath.Application.DTOs;

namespace Golyath.Application.Services;

public interface ISuggestionsService
{
    /// <summary>
    /// Analyses the user's training history and returns a ranked list of actionable suggestions.
    /// </summary>
    Task<IReadOnlyList<TrainingSuggestion>> GetSuggestionsAsync();
}
