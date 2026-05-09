namespace Golyath.Application.DTOs;

public enum SuggestionType
{
    IncreaseWeight,
    IncreaseReps,
    PlateauDetected,
    MuscleImbalance,
    UndertrainedMuscle,
    Deload
}

public enum SuggestionPriority
{
    Low,
    Medium,
    High
}

/// <summary>A single actionable training recommendation produced by the suggestions engine.</summary>
public record TrainingSuggestion(
    SuggestionType Type,
    SuggestionPriority Priority,
    string Title,
    string Detail,
    string Icon)
{
    public string PriorityLabel => Priority switch
    {
        SuggestionPriority.High   => "HIGH",
        SuggestionPriority.Medium => "MEDIUM",
        _                         => "LOW"
    };

    public Color PriorityColor => Priority switch
    {
        SuggestionPriority.High   => Color.FromArgb("#E53935"),
        SuggestionPriority.Medium => Color.FromArgb("#FB8C00"),
        _                         => Color.FromArgb("#FFD700")
    };
}
