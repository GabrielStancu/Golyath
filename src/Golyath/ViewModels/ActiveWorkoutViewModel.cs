using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Models;
using Golyath.Services;
using System.Collections.ObjectModel;

namespace Golyath.ViewModels;

/// <summary>Represents one exercise + its logged sets within an active workout session.</summary>
public partial class ActiveExerciseItem : ObservableObject
{
    public Exercise Exercise { get; init; } = null!;

    [ObservableProperty]
    private ObservableCollection<WorkoutSet> _sets = [];
}

public partial class ActiveWorkoutViewModel : BaseViewModel
{
    private readonly IWorkoutService _workoutService;
    private readonly IExerciseService _exerciseService;

    private WorkoutSession? _currentSession;
    private System.Timers.Timer? _elapsedTimer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSessionIdle))]
    private bool _isSessionActive;

    public bool IsSessionIdle => !IsSessionActive;

    [ObservableProperty]
    private string _elapsedTime = "00:00";

    [ObservableProperty]
    private ObservableCollection<ActiveExerciseItem> _exerciseItems = [];

    [ObservableProperty]
    private ObservableCollection<Exercise> _availableExercises = [];

    [ObservableProperty]
    private bool _isPickerVisible;

    [ObservableProperty]
    private string _exercisePickerSearch = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Exercise> _filteredPickerExercises = [];

    public ActiveWorkoutViewModel(IWorkoutService workoutService, IExerciseService exerciseService)
    {
        _workoutService = workoutService;
        _exerciseService = exerciseService;
        Title = "Workout";
    }

    [RelayCommand]
    private async Task StartWorkoutAsync()
    {
        _currentSession = await _workoutService.StartSessionAsync();
        IsSessionActive = true;
        StartElapsedTimer();
    }

    [RelayCommand]
    private async Task FinishWorkoutAsync()
    {
        if (_currentSession is null) return;

        _elapsedTimer?.Stop();
        _elapsedTimer?.Dispose();
        _elapsedTimer = null;

        await _workoutService.FinishSessionAsync(_currentSession);

        _currentSession = null;
        IsSessionActive = false;
        ElapsedTime = "00:00";
        ExerciseItems.Clear();
    }

    [RelayCommand]
    private async Task ShowExercisePickerAsync()
    {
        if (_availableExercises.Count == 0)
        {
            var all = await _exerciseService.GetAllExercisesAsync();
            _availableExercises = new ObservableCollection<Exercise>(all.OrderBy(e => e.Name));
        }
        FilteredPickerExercises = new ObservableCollection<Exercise>(_availableExercises);
        ExercisePickerSearch = string.Empty;
        IsPickerVisible = true;
    }

    [RelayCommand]
    private void HideExercisePicker() => IsPickerVisible = false;

    partial void OnExercisePickerSearchChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FilteredPickerExercises = new ObservableCollection<Exercise>(_availableExercises);
        }
        else
        {
            var filtered = _availableExercises
                .Where(e => e.Name.Contains(value, StringComparison.OrdinalIgnoreCase));
            FilteredPickerExercises = new ObservableCollection<Exercise>(filtered);
        }
    }

    [RelayCommand]
    private void AddExercise(Exercise exercise)
    {
        IsPickerVisible = false;
        var existing = ExerciseItems.FirstOrDefault(i => i.Exercise.Id == exercise.Id);
        if (existing is null)
            ExerciseItems.Add(new ActiveExerciseItem { Exercise = exercise });
    }

    [RelayCommand]
    private async Task AddSetAsync(ActiveExerciseItem item)
    {
        if (_currentSession is null) return;

        int setNumber = item.Sets.Count + 1;
        // Default: same weight/reps as last set for convenience
        double weight = item.Sets.LastOrDefault()?.Weight ?? 0;
        int reps = item.Sets.LastOrDefault()?.Reps ?? 10;

        var set = await _workoutService.AddSetAsync(
            _currentSession.Id, item.Exercise.Id, setNumber, weight, reps);
        item.Sets.Add(set);
    }

    [RelayCommand]
    private async Task CompleteSetAsync(WorkoutSet set)
    {
        set.CompletedAt = DateTime.UtcNow;
        await _workoutService.UpdateSetAsync(set);
    }

    [RelayCommand]
    private async Task DeleteSetAsync((ActiveExerciseItem Item, WorkoutSet Set) args)
    {
        await _workoutService.DeleteSetAsync(args.Set);
        args.Item.Sets.Remove(args.Set);
    }

    private void StartElapsedTimer()
    {
        var started = DateTime.UtcNow;
        _elapsedTimer = new System.Timers.Timer(1000);
        _elapsedTimer.Elapsed += (_, _) =>
        {
            var elapsed = DateTime.UtcNow - started;
            MainThread.BeginInvokeOnMainThread(() =>
                ElapsedTime = elapsed.ToString(@"mm\:ss"));
        };
        _elapsedTimer.Start();
    }
}
