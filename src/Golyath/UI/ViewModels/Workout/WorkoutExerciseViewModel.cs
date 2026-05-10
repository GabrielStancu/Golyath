using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.Services;
using Golyath.Core.Entities;

namespace Golyath.UI.ViewModels.Workout;

public partial class WorkoutExerciseViewModel : ObservableObject
{
    private readonly IWorkoutService _workoutService;
    private readonly WorkoutExercise _workoutExercise;

    public int WorkoutExerciseId { get; }
    public string ExerciseName { get; }
    public int ExerciseId { get; }

    [ObservableProperty]
    private string _exerciseNotes;

    public ObservableCollection<WorkoutSetViewModel> Sets { get; } = [];

    public event EventHandler<WorkoutSetViewModel>? SetCompleted;
    public event EventHandler<WorkoutExerciseViewModel>? RemoveRequested;

    public WorkoutExerciseViewModel(WorkoutExercise workoutExercise, Exercise exercise, IWorkoutService workoutService)
    {
        _workoutExercise = workoutExercise;
        WorkoutExerciseId = workoutExercise.Id;
        ExerciseName = exercise.Name;
        ExerciseId = exercise.Id;
        _workoutService = workoutService;
        _exerciseNotes = workoutExercise.Notes ?? string.Empty;
    }

    partial void OnExerciseNotesChanged(string value)
    {
        _workoutExercise.Notes = string.IsNullOrWhiteSpace(value) ? null : value;
        _ = _workoutService.UpdateExerciseNotesAsync(WorkoutExerciseId, _workoutExercise.Notes);
    }

    public void LoadSets(IEnumerable<WorkoutSet> sets)
    {
        Sets.Clear();
        foreach (var set in sets)
            AddSetViewModel(set);
    }

    private WorkoutSetViewModel AddSetViewModel(WorkoutSet set)
    {
        var vm = new WorkoutSetViewModel(set, _workoutService);
        vm.SetCompleted += (_, s) => SetCompleted?.Invoke(this, s);
        vm.DuplicateRequested += OnDuplicateSet;
        Sets.Add(vm);
        return vm;
    }

    [RelayCommand]
    private async Task AddSet()
    {
        var weight = Sets.LastOrDefault()?.WeightText is { } wt
            && double.TryParse(wt, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var w) ? w : 0;
        var reps = Sets.LastOrDefault()?.RepsText is { } rt
            && int.TryParse(rt, out var r) ? r : 10;

        var set = await _workoutService.AddSetAsync(WorkoutExerciseId, weight, reps);
        AddSetViewModel(set);
    }

    private async void OnDuplicateSet(object? sender, WorkoutSetViewModel original)
    {
        var set = await _workoutService.DuplicateSetAsync(original.Id);
        AddSetViewModel(set);
    }

    [RelayCommand]
    private void Remove() => RemoveRequested?.Invoke(this, this);
}
