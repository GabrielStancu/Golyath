using CommunityToolkit.Mvvm.ComponentModel;
using Golyath.Application.DTOs;
using Golyath.Application.Services;

namespace Golyath.UI.ViewModels.History;

/// <summary>Mutable VM wrapping a single set row in the workout history detail view.</summary>
public partial class HistorySetViewModel : ObservableObject
{
    private readonly IWorkoutService _workoutService;

    public int SetId { get; }
    public int SetNumber { get; }

    [ObservableProperty] private string _weightText;
    [ObservableProperty] private string _repsText;
    [ObservableProperty] private string _tempoText;
    [ObservableProperty] private string _notesText;

    public HistorySetViewModel(SetSummaryDto dto, IWorkoutService workoutService)
    {
        SetId = dto.SetId;
        SetNumber = dto.SetNumber;
        _workoutService = workoutService;
        _weightText = dto.Weight > 0 ? dto.Weight.ToString("0.#") : string.Empty;
        _repsText = dto.Reps > 0 ? dto.Reps.ToString() : string.Empty;
        _tempoText = dto.Tempo ?? string.Empty;
        _notesText = dto.Notes ?? string.Empty;
    }

    private void Save()
    {
        if (!double.TryParse(WeightText, out var weight)) weight = 0;
        if (!int.TryParse(RepsText, out var reps)) reps = 0;
        _ = _workoutService.UpdateSetFieldsAsync(
            SetId, weight, reps,
            string.IsNullOrWhiteSpace(TempoText) ? null : TempoText,
            string.IsNullOrWhiteSpace(NotesText) ? null : NotesText);
    }

    partial void OnWeightTextChanged(string value) => Save();
    partial void OnRepsTextChanged(string value) => Save();
    partial void OnTempoTextChanged(string value) => Save();
    partial void OnNotesTextChanged(string value) => Save();
}
