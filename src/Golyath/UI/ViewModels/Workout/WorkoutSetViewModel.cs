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
        "2-0-2-0",
        "3-1-2-0",
        "3-0-1-0",
        "4-0-1-0",
        "4-1-1-0",
        "5-0-1-0",
        "2-1-2-1",
        "1-0-1-0",
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

    [ObservableProperty] private string _weightText;
    [ObservableProperty] private string _repsText;
    [ObservableProperty] private string _tempoText;
    [ObservableProperty] private string _notesText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanComplete))]
    private bool _isCompleted;

    // Focus state — controlled by WorkoutExerciseViewModel
    [ObservableProperty] private bool _isActive;

    // Previous session data — set by parent after loading history
    [ObservableProperty] private string _prevDisplay = "—";

    public bool CanComplete => !IsCompleted;

    public event EventHandler<WorkoutSetViewModel>? SetCompleted;
    public event EventHandler<WorkoutSetViewModel>? DuplicateRequested;
    public event EventHandler<WorkoutSetViewModel>? RemoveRequested;
    public event EventHandler<WorkoutSetViewModel>? FocusRequested;
    public event EventHandler<double>?              WeightChanged;

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
            WeightChanged?.Invoke(this, w);
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

    // Public so ActiveWorkoutViewModel can await it from LogAndContinueCommand
    public async Task CompleteAsync()
    {
        if (IsCompleted) return;
        _set = await _workoutService.CompleteSetAsync(_set.Id);
        IsCompleted = true;
        SetCompleted?.Invoke(this, this);
    }

    [RelayCommand(CanExecute = nameof(CanComplete))]
    private Task CompleteSet() => CompleteAsync();

    [RelayCommand]
    private void Focus() => FocusRequested?.Invoke(this, this);

    [RelayCommand]
    private void Duplicate() => DuplicateRequested?.Invoke(this, this);

    [RelayCommand]
    private void Remove() => RemoveRequested?.Invoke(this, this);

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
