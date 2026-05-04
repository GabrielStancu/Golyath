using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.Services;
using Golyath.Core.Entities;

namespace Golyath.UI.ViewModels.Workout;

public partial class WorkoutSetViewModel : ObservableObject
{
    private readonly IWorkoutService _workoutService;
    private WorkoutSet _set;

    public WorkoutSetViewModel(WorkoutSet set, IWorkoutService workoutService)
    {
        _set = set;
        _workoutService = workoutService;

        _weightText = set.Weight.ToString("F1", CultureInfo.InvariantCulture);
        _repsText = set.Reps.ToString();
        _isCompleted = set.IsCompleted;
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

    public bool CanComplete => !IsCompleted;

    public event EventHandler<WorkoutSetViewModel>? SetCompleted;
    public event EventHandler<WorkoutSetViewModel>? DuplicateRequested;

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
}
