using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.Services;
using Golyath.Core.Entities;

namespace Golyath.UI.ViewModels.Workout;

public partial class WorkoutExerciseViewModel : ObservableObject
{
    private readonly IWorkoutService _workoutService;
    private readonly WorkoutExercise _workoutExercise;
    private WorkoutSetViewModel? _activeSet;
    private double _historicalMaxWeight;

    public int WorkoutExerciseId { get; }
    public string ExerciseName { get; }
    public int ExerciseId { get; }

    [ObservableProperty] private string _exerciseNotes;
    [ObservableProperty] private bool _showPrBadge;
    [ObservableProperty] private string _prBadgeText = string.Empty;

    public bool IsFullyCompleted => Sets.Count > 0 && Sets.All(s => s.IsCompleted);

    public ObservableCollection<WorkoutSetViewModel> Sets { get; } = [];

    public event EventHandler<WorkoutSetViewModel>?                                           SetCompleted;
    public event EventHandler<WorkoutExerciseViewModel>?                                      RemoveRequested;
    public event EventHandler<(WorkoutExerciseViewModel Exercise, WorkoutSetViewModel Set)>?  SetFocusRequested;

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

    /// <summary>Populates PrevDisplay on each set row from the previous session's data.</summary>
    public void SetPreviousSets(IReadOnlyList<WorkoutSet> prevSets)
    {
        var ordered = prevSets.OrderBy(s => s.SetNumber).ToList();
        _historicalMaxWeight = ordered.Count > 0 ? ordered.Max(s => s.Weight) : 0;

        for (int i = 0; i < Sets.Count; i++)
            Sets[i].PrevDisplay = i < ordered.Count
                ? $"{ordered[i].Weight:0.#}×{ordered[i].Reps}"
                : "—";
    }

    /// <summary>Set this exercise's active (focused) set; handles PR badge wiring.</summary>
    public void FocusSet(WorkoutSetViewModel target)
    {
        if (_activeSet is not null)
        {
            _activeSet.IsActive = false;
            _activeSet.WeightChanged -= OnActiveSetWeightChanged;
        }
        _activeSet = target;
        target.IsActive = true;
        target.WeightChanged += OnActiveSetWeightChanged;

        if (double.TryParse(target.WeightText, NumberStyles.Any,
            CultureInfo.InvariantCulture, out var w))
            UpdatePrBadge(w);
    }

    /// <summary>Remove focus from this exercise entirely (called when another exercise is selected).</summary>
    public void ClearFocus()
    {
        if (_activeSet is not null)
        {
            _activeSet.IsActive = false;
            _activeSet.WeightChanged -= OnActiveSetWeightChanged;
            _activeSet = null;
        }
        ShowPrBadge = false;
    }

    private void OnActiveSetWeightChanged(object? sender, double weight) => UpdatePrBadge(weight);

    private void UpdatePrBadge(double weight)
    {
        if (weight > 0 && _historicalMaxWeight > 0 && weight > _historicalMaxWeight)
        {
            ShowPrBadge = true;
            PrBadgeText = $"PR · {weight:0.#} KG";
        }
        else
        {
            ShowPrBadge = false;
        }
    }

    private WorkoutSetViewModel AddSetViewModel(WorkoutSet set)
    {
        var vm = new WorkoutSetViewModel(set, _workoutService);
        vm.SetCompleted += (_, s) =>
        {
            SetCompleted?.Invoke(this, s);
            OnPropertyChanged(nameof(IsFullyCompleted));
        };
        vm.FocusRequested += (_, s) => SetFocusRequested?.Invoke(this, (this, s));
        vm.DuplicateRequested += OnDuplicateSet;
        vm.RemoveRequested += OnRemoveSet;
        Sets.Add(vm);
        return vm;
    }

    [RelayCommand]
    private async Task AddSet()
    {
        var weight = Sets.LastOrDefault()?.WeightText is { } wt
            && double.TryParse(wt, NumberStyles.Any, CultureInfo.InvariantCulture, out var w) ? w : 0;
        var reps = Sets.LastOrDefault()?.RepsText is { } rt
            && int.TryParse(rt, out var r) ? r : 10;

        var set = await _workoutService.AddSetAsync(WorkoutExerciseId, weight, reps);
        var vm = AddSetViewModel(set);
        vm.PrevDisplay = "—";
    }

    private async void OnDuplicateSet(object? sender, WorkoutSetViewModel original)
    {
        var set = await _workoutService.DuplicateSetAsync(original.Id);
        var vm = AddSetViewModel(set);
        vm.PrevDisplay = "—";
    }

    private async void OnRemoveSet(object? sender, WorkoutSetViewModel setVm)
    {
        await _workoutService.RemoveSetAsync(setVm.Id);
        if (_activeSet == setVm) ClearFocus();
        Sets.Remove(setVm);
        OnPropertyChanged(nameof(IsFullyCompleted));
    }

    [RelayCommand]
    private void Remove() => RemoveRequested?.Invoke(this, this);
}
