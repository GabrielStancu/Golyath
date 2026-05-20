using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Golyath.Application.Services;
using Golyath.Core.Enums;
using Golyath.Core.Entities;

namespace Golyath.UI.ViewModels.Workout;

public partial class RoutineBuilderViewModel : ObservableObject, IRecipient<ExercisePickedMessage>, IQueryAttributable
{
    private readonly IRoutineService _routineService;
    private int? _routineId;

    [ObservableProperty] private string _routineName = string.Empty;
    [ObservableProperty] private RoutineCategory _selectedCategory = RoutineCategory.Custom;
    [ObservableProperty] private bool _isEditing;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _pageTitle = "Build routine";
    [ObservableProperty] private string _muscleSummary = string.Empty;

    public ObservableCollection<RoutineExerciseItemViewModel> Exercises { get; } = [];

    // ── Muscle chip system ──────────────────────────────────────────────────
    public static readonly string[] AllMuscles = ["Chest", "Shoulders", "Triceps", "Back", "Biceps", "Legs", "Core"];
    public ObservableCollection<MuscleChipViewModel> MuscleChips { get; } = [];
    private readonly HashSet<string> _selectedMuscles = [];
    public IReadOnlySet<string> SelectedMusclesSet => _selectedMuscles;

    // Raised when selection changes so the view can update the BodyMapView
    public event EventHandler? MuscleSelectionChanged;

    public RoutineBuilderViewModel(IRoutineService routineService)
    {
        _routineService = routineService;
        foreach (var m in AllMuscles)
            MuscleChips.Add(new MuscleChipViewModel { Name = m });
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("routineId", out var idObj) && idObj is string idStr && int.TryParse(idStr, out var id))
        {
            _routineId = id;
            IsEditing = true;
            PageTitle = "Edit routine";
        }
    }

    public void RegisterMessenger()
    {
        WeakReferenceMessenger.Default.Unregister<ExercisePickedMessage>(this);
        WeakReferenceMessenger.Default.Register(this);
    }

    public void UnregisterMessenger() =>
        WeakReferenceMessenger.Default.Unregister<ExercisePickedMessage>(this);

    public async void Receive(ExercisePickedMessage message)
    {
        var exercise = message.Value;
        Exercises.Add(new RoutineExerciseItemViewModel
        {
            ExerciseId = exercise.Id,
            ExerciseName = exercise.Name,
            Order = Exercises.Count,
            TargetSets = 3,
            TargetReps = 10,
            RestSeconds = 90
        });
        await Task.CompletedTask;
    }

    public async Task LoadAsync()
    {
        if (_routineId is not { } id) return;

        IsBusy = true;
        try
        {
            var detail = await _routineService.GetRoutineDetailAsync(id);
            if (detail is null) return;

            RoutineName = detail.Name;
            SelectedCategory = detail.Category;
            Exercises.Clear();

            foreach (var e in detail.Exercises)
            {
                Exercises.Add(new RoutineExerciseItemViewModel
                {
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.ExerciseName,
                    Order = e.Order,
                    TargetSets = e.TargetSets,
                    TargetReps = e.TargetReps,
                    TargetWeight = e.TargetWeight,
                    RestSeconds = e.RestSeconds
                });
            }

            // Restore muscle selection from category
            SetMusclesFromCategory(detail.Category);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Muscle chip toggling ────────────────────────────────────────────────

    [RelayCommand]
    private void ToggleMuscle(MuscleChipViewModel chip)
    {
        chip.IsSelected = !chip.IsSelected;
        if (chip.IsSelected)
            _selectedMuscles.Add(chip.Name);
        else
            _selectedMuscles.Remove(chip.Name);

        SelectedCategory = DetectCategory();
        UpdateMuscleSummary();
        MuscleSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SetMusclesFromCategory(RoutineCategory category)
    {
        _selectedMuscles.Clear();
        var muscles = category switch
        {
            RoutineCategory.Push => new[] { "Chest", "Shoulders", "Triceps" },
            RoutineCategory.Pull => new[] { "Back", "Biceps" },
            RoutineCategory.Legs => new[] { "Legs" },
            RoutineCategory.Upper => new[] { "Chest", "Shoulders", "Triceps", "Back", "Biceps" },
            RoutineCategory.Lower => new[] { "Legs" },
            RoutineCategory.FullBody => new[] { "Chest", "Shoulders", "Triceps", "Back", "Biceps", "Legs", "Core" },
            RoutineCategory.Core => new[] { "Core" },
            _ => Array.Empty<string>()
        };
        foreach (var m in muscles) _selectedMuscles.Add(m);
        foreach (var chip in MuscleChips)
            chip.IsSelected = _selectedMuscles.Contains(chip.Name);
        UpdateMuscleSummary();
        MuscleSelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private RoutineCategory DetectCategory()
    {
        if (_selectedMuscles.SetEquals(["Chest", "Shoulders", "Triceps"])) return RoutineCategory.Push;
        if (_selectedMuscles.SetEquals(["Back", "Biceps"])) return RoutineCategory.Pull;
        if (_selectedMuscles.SetEquals(["Legs"])) return RoutineCategory.Legs;
        if (_selectedMuscles.SetEquals(["Chest", "Shoulders", "Triceps", "Back", "Biceps"])) return RoutineCategory.Upper;
        if (_selectedMuscles.SetEquals(["Chest", "Shoulders", "Triceps", "Back", "Biceps", "Legs", "Core"])) return RoutineCategory.FullBody;
        if (_selectedMuscles.SetEquals(["Core"])) return RoutineCategory.Core;
        if (_selectedMuscles.Count == 0) return RoutineCategory.Custom;
        // Heuristic: if contains legs + upper body → Full Body
        if (_selectedMuscles.Contains("Legs") && _selectedMuscles.Any(m => m is "Chest" or "Back" or "Shoulders"))
            return RoutineCategory.FullBody;
        if (_selectedMuscles.Contains("Legs")) return RoutineCategory.Legs;
        return RoutineCategory.Custom;
    }

    private void UpdateMuscleSummary()
    {
        if (_selectedMuscles.Count == 0)
        {
            MuscleSummary = string.Empty;
            return;
        }
        var cat = SelectedCategory.DisplayName().ToUpperInvariant();
        var muscles = string.Join(" · ", _selectedMuscles.Select(m => m.ToUpperInvariant()));
        MuscleSummary = $"{cat}: {muscles}";
    }

    // ── Exercise list ───────────────────────────────────────────────────────

    [RelayCommand]
    private void RequestAddExercise()
    {
        AddExerciseRequested?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? AddExerciseRequested;

    [RelayCommand]
    private void MoveUp(RoutineExerciseItemViewModel item)
    {
        var idx = Exercises.IndexOf(item);
        if (idx <= 0) return;
        Exercises.Move(idx, idx - 1);
        ReorderIndices();
    }

    [RelayCommand]
    private void MoveDown(RoutineExerciseItemViewModel item)
    {
        var idx = Exercises.IndexOf(item);
        if (idx < 0 || idx >= Exercises.Count - 1) return;
        Exercises.Move(idx, idx + 1);
        ReorderIndices();
    }

    [RelayCommand]
    private async Task RemoveExercise(RoutineExerciseItemViewModel item)
    {
        bool confirm = await Shell.Current.DisplayAlert("Remove Exercise",
            $"Remove \"{item.ExerciseName}\" from this routine?", "Remove", "Cancel");
        if (!confirm) return;

        Exercises.Remove(item);
        ReorderIndices();
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(RoutineName)) return;

        IsBusy = true;
        try
        {
            if (_routineId is not null)
            {
                await _routineService.UpdateRoutineAsync(_routineId.Value, RoutineName.Trim(), SelectedCategory);
            }
            else
            {
                var routine = await _routineService.CreateRoutineAsync(RoutineName.Trim(), SelectedCategory);
                _routineId = routine.Id;
            }

            var inputs = Exercises.Select((e, i) => new RoutineExerciseInput(
                e.ExerciseId, i, e.TargetSets, e.TargetReps, e.TargetWeight, e.RestSeconds)).ToList();

            await _routineService.SetRoutineExercisesAsync(_routineId!.Value, inputs);

            WeakReferenceMessenger.Default.Send(new RoutineChangedMessage());
            UnregisterMessenger();
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRoutine()
    {
        if (_routineId is not { } id) return;

        bool confirm = await Shell.Current.DisplayAlert("Delete Routine",
            "Are you sure you want to delete this routine?", "Delete", "Cancel");
        if (!confirm) return;

        await _routineService.DeleteRoutineAsync(id);
        WeakReferenceMessenger.Default.Send(new RoutineChangedMessage());
        UnregisterMessenger();
        await Shell.Current.GoToAsync("..");
    }

    private void ReorderIndices()
    {
        for (int i = 0; i < Exercises.Count; i++)
            Exercises[i].Order = i;
    }
}

public partial class MuscleChipViewModel : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private bool _isSelected;
}

public partial class RoutineExerciseItemViewModel : ObservableObject
{
    [ObservableProperty] private int _exerciseId;
    [ObservableProperty] private string _exerciseName = string.Empty;
    [ObservableProperty] private int _order;
    [ObservableProperty] private int _targetSets = 3;
    [ObservableProperty] private int _targetReps = 10;
    [ObservableProperty] private double? _targetWeight;
    [ObservableProperty] private int _restSeconds = 90;

    partial void OnRestSecondsChanged(int value) => OnPropertyChanged(nameof(RestDisplay));

    public string RestDisplay => $"{RestSeconds}s";

    /// <summary>Text for the weight Entry. Null/0 shows empty placeholder.</summary>
    public string WeightText
    {
        get => _targetWeight is > 0 ? _targetWeight.Value.ToString("G") : string.Empty;
        set
        {
            if (double.TryParse(value, out var v) && v >= 0)
                TargetWeight = v > 0 ? v : null;
            else
                TargetWeight = null;
            OnPropertyChanged();
        }
    }

    partial void OnTargetWeightChanged(double? value) => OnPropertyChanged(nameof(WeightText));
}

/// <summary>Sent when a routine is created, updated, or deleted.</summary>
public sealed class RoutineChangedMessage;
