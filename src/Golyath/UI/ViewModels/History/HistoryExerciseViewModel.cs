using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Golyath.Application.DTOs;
using Golyath.Application.Services;

namespace Golyath.UI.ViewModels.History;

/// <summary>Mutable VM wrapping an exercise block in the workout history detail view.</summary>
public partial class HistoryExerciseViewModel : ObservableObject
{
    private readonly IWorkoutService _workoutService;
    private bool _initializing;

    public int WorkoutExerciseId { get; }
    public string ExerciseName { get; }
    public string SetsSummary { get; }

    [ObservableProperty] private string _exerciseNotes;

    public ObservableCollection<HistorySetViewModel> Sets { get; } = [];

    public HistoryExerciseViewModel(WorkoutExerciseSummaryDto dto, IWorkoutService workoutService)
    {
        WorkoutExerciseId = dto.WorkoutExerciseId;
        ExerciseName = dto.ExerciseName;
        SetsSummary = dto.SetsSummary;
        _workoutService = workoutService;

        _initializing = true;
        _exerciseNotes = dto.ExerciseNotes ?? string.Empty;
        _initializing = false;

        foreach (var s in dto.Sets)
            Sets.Add(new HistorySetViewModel(s, workoutService));
    }

    partial void OnExerciseNotesChanged(string value)
    {
        if (_initializing) return;
        var notes = string.IsNullOrWhiteSpace(value) ? null : value;
        _ = _workoutService.UpdateExerciseNotesAsync(WorkoutExerciseId, notes);
    }
}
