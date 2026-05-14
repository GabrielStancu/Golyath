using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.Services;
using Golyath.Core.Entities;
using Golyath.UI.Controls;

namespace Golyath.UI.ViewModels.Workout;

public partial class WorkoutSetViewModel : ObservableObject
{
    private readonly IWorkoutService _workoutService;
    private WorkoutSet _set;

    public static readonly IReadOnlyList<string> TempoOptions =
    [
        "2-0-2-0",   // Standard
        "3-1-2-0",   // Controlled
        "3-0-1-0",   // Explosive concentric
        "4-0-1-0",   // Slow eccentric
        "4-1-1-0",   // Time under tension
        "5-0-1-0",   // Heavy negative
        "2-1-2-1",   // Pause reps
        "1-0-1-0",   // Fast / Power
        "None",
    ];

    public WorkoutSetViewModel(WorkoutSet set, IWorkoutService workoutService)
    {
        _set = set;
        _workoutService = workoutService;

        _weightText = set.Weight.ToString("F1", CultureInfo.InvariantCulture);
        _repsText = set.Reps.ToString();
        _tempoText = set.Tempo ?? string.Empty;
        _isCompleted = set.IsCompleted;
        _notesText = set.Notes ?? string.Empty;
    }

    public int Id => _set.Id;
    public int WorkoutExerciseId => _set.WorkoutExerciseId;
    public int SetNumber => _set.SetNumber;

    [ObservableProperty]
    private string _weightText;

    [ObservableProperty]
    private string _repsText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanComplete))]
    private bool _isCompleted;

    [ObservableProperty]
    private string _tempoText;

    [ObservableProperty]
    private string _notesText;

    public bool CanComplete => !IsCompleted;

    public event EventHandler<WorkoutSetViewModel>? SetCompleted;
    public event EventHandler<WorkoutSetViewModel>? DuplicateRequested;

    partial void OnTempoTextChanged(string value)
    {
        _set.Tempo = string.IsNullOrWhiteSpace(value) ? null : value;
        _ = _workoutService.UpdateSetAsync(_set);
        OnPropertyChanged(nameof(TempoDisplay));
    }

    partial void OnNotesTextChanged(string value)
    {
        _set.Notes = string.IsNullOrWhiteSpace(value) ? null : value;
        _ = _workoutService.UpdateSetAsync(_set);
    }

    partial void OnWeightTextChanged(string value)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var w)
            && Math.Abs(w - _set.Weight) > 0.001)
        {
            _set.Weight = w;
            _ = _workoutService.UpdateSetAsync(_set);
        }
    }

    partial void OnRepsTextChanged(string value)
    {
        if (int.TryParse(value, out var r) && r > 0 && r != _set.Reps)
        {
            _set.Reps = r;
            _ = _workoutService.UpdateSetAsync(_set);
        }
    }

    [RelayCommand(CanExecute = nameof(CanComplete))]
    private async Task CompleteSet()
    {
        _set = await _workoutService.CompleteSetAsync(_set.Id);
        IsCompleted = true;
        SetCompleted?.Invoke(this, this);
    }

    [RelayCommand]
    private void Duplicate() => DuplicateRequested?.Invoke(this, this);

    [RelayCommand]
    private void IncrementWeight()
    {
        var current = double.TryParse(WeightText, NumberStyles.Any, CultureInfo.InvariantCulture, out var w) ? w : 0;
        WeightText = Math.Round(current + 2.5, 2).ToString("F1", CultureInfo.InvariantCulture);
    }

    [RelayCommand]
    private void DecrementWeight()
    {
        var current = double.TryParse(WeightText, NumberStyles.Any, CultureInfo.InvariantCulture, out var w) ? w : 0;
        if (current >= 2.5)
            WeightText = Math.Round(current - 2.5, 2).ToString("F1", CultureInfo.InvariantCulture);
    }

    [RelayCommand]
    private async Task SelectTempo()
    {
        var popup = new SelectionPopup("Tempo", TempoOptions, string.IsNullOrEmpty(TempoText) ? null : TempoText);
        var result = await popup.ShowAsync();
        if (result is string selected)
            TempoText = selected == "None" ? string.Empty : selected;
    }

    public string TempoDisplay => string.IsNullOrEmpty(TempoText) ? "—" : TempoText;
}
