using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Core.Enums;

namespace Golyath.UI.ViewModels.Exercises;

public partial class ExerciseLibraryViewModel : ObservableObject
{
    private readonly IExerciseRepository _exerciseRepository;

    // Pre-computed per-exercise search data built once on load
    private record ExerciseEntry(Exercise Exercise, string NameLower, MuscleGroup[] AllMuscles);
    private IReadOnlyList<ExerciseEntry> _index = [];

    private bool _applyingFilter;
    private CancellationTokenSource? _searchCts;

    // Last non-null user selections — restored when Picker transiently sets SelectedItem=null
    private string _lastValidMuscle = "All";
    private string _lastValidEquipment = "All";

    // ── Bindable collections as full-replace List<T> to avoid ObservableCollection.Clear()
    //    which sends CollectionChanged.Reset and causes Picker to clear SelectedItem.

    private List<Exercise> _exercises = [];
    public List<Exercise> Exercises
    {
        get => _exercises;
        private set => SetProperty(ref _exercises, value);
    }

    private List<string> _muscleGroupOptions = ["All"];
    public List<string> MuscleGroupOptions
    {
        get => _muscleGroupOptions;
        private set => SetProperty(ref _muscleGroupOptions, value);
    }

    private List<string> _equipmentOptions = ["All"];
    public List<string> EquipmentOptions
    {
        get => _equipmentOptions;
        private set => SetProperty(ref _equipmentOptions, value);
    }

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedMuscleGroup = "All";

    [ObservableProperty]
    private string _selectedEquipment = "All";

    [ObservableProperty]
    private bool _isBusy;

    private MuscleGroup? ActiveMuscle =>
        SelectedMuscleGroup is not (null or "All") && Enum.TryParse<MuscleGroup>(SelectedMuscleGroup, out var m)
            ? m : null;

    private EquipmentType? ActiveEquipment =>
        SelectedEquipment is not (null or "All") && Enum.TryParse<EquipmentType>(SelectedEquipment, out var e)
            ? e : null;

    public ExerciseLibraryViewModel(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var all = await _exerciseRepository.GetAllAsync();
            _index = all.Select(e => new ExerciseEntry(
                e,
                e.Name.ToLowerInvariant(),
                new[] { e.PrimaryMuscle }.Concat(e.SecondaryMuscles).ToArray()
            )).ToList();
            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Debounce search: wait 300 ms after the last keystroke before filtering
    partial void OnSearchQueryChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var cts = _searchCts;
        _ = Task.Delay(300, cts.Token).ContinueWith(
            t => { if (!t.IsCanceled) ApplyFilter(); },
            CancellationToken.None,
            TaskContinuationOptions.None,
            TaskScheduler.FromCurrentSynchronizationContext());
    }

    partial void OnSelectedMuscleGroupChanged(string? value)
    {
        if (value is null)
        {
            // Picker transiently clears SelectedItem when its ItemsSource property changes —
            // restore the last known-good value instead of jumping to "All".
            SelectedMuscleGroup = _lastValidMuscle;
            return;
        }
        _lastValidMuscle = value;
        ApplyFilter();
    }

    partial void OnSelectedEquipmentChanged(string? value)
    {
        if (value is null)
        {
            SelectedEquipment = _lastValidEquipment;
            return;
        }
        _lastValidEquipment = value;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (_index.Count == 0 || _applyingFilter) return;
        _applyingFilter = true;
        try
        {
            var query = SearchQuery.Trim().ToLowerInvariant();
            var muscle = ActiveMuscle;
            var equipment = ActiveEquipment;

            UpdateAvailableOptions(query, muscle, equipment);

            // Re-read after options update may have reset selections
            muscle = ActiveMuscle;
            equipment = ActiveEquipment;

            var filtered = _index.AsEnumerable();
            if (!string.IsNullOrEmpty(query))
                filtered = filtered.Where(e => e.NameLower.Contains(query));
            if (muscle is not null)
                filtered = filtered.Where(e => e.AllMuscles.Contains(muscle.Value));
            if (equipment is not null)
                filtered = filtered.Where(e => e.Exercise.Equipment == equipment.Value);

            // Assign new list — single PropertyChanged, no N×CollectionChanged
            Exercises = filtered.OrderBy(e => e.Exercise.Name).Select(e => e.Exercise).ToList();
        }
        finally
        {
            _applyingFilter = false;
        }
    }

    private void UpdateAvailableOptions(string query, MuscleGroup? currentMuscle, EquipmentType? currentEquipment)
    {
        var savedMuscle = SelectedMuscleGroup ?? "All";
        var savedEquipment = SelectedEquipment ?? "All";

        // Muscles available = primary + secondary of exercises matching search & current equipment
        var forMuscle = _index.AsEnumerable();
        if (!string.IsNullOrEmpty(query))
            forMuscle = forMuscle.Where(e => e.NameLower.Contains(query));
        if (currentEquipment is not null)
            forMuscle = forMuscle.Where(e => e.Exercise.Equipment == currentEquipment.Value);

        var availableMuscles = forMuscle
            .SelectMany(e => e.AllMuscles)
            .Select(m => m.ToString())
            .Distinct()
            .ToHashSet();

        // Equipment available = distinct equipment matching search & current muscle
        var forEquip = _index.AsEnumerable();
        if (!string.IsNullOrEmpty(query))
            forEquip = forEquip.Where(e => e.NameLower.Contains(query));
        if (currentMuscle is not null)
            forEquip = forEquip.Where(e => e.AllMuscles.Contains(currentMuscle.Value));

        var availableEquipment = forEquip
            .Select(e => e.Exercise.Equipment.ToString())
            .Distinct()
            .ToHashSet();

        // Replace full list (PropertyChanged) instead of Clear()+Add (CollectionChanged.Reset)
        var desiredMuscles = new[] { "All" }.Concat(availableMuscles.OrderBy(s => s)).ToList();
        if (!MuscleGroupOptions.SequenceEqual(desiredMuscles))
            MuscleGroupOptions = desiredMuscles;

        var desiredEquipment = new[] { "All" }.Concat(availableEquipment.OrderBy(s => s)).ToList();
        if (!EquipmentOptions.SequenceEqual(desiredEquipment))
            EquipmentOptions = desiredEquipment;

        // Reset selections that are no longer valid; update _lastValid to match
        if (savedMuscle != "All" && !availableMuscles.Contains(savedMuscle))
        {
            _lastValidMuscle = "All";
            SelectedMuscleGroup = "All";
        }
        if (savedEquipment != "All" && !availableEquipment.Contains(savedEquipment))
        {
            _lastValidEquipment = "All";
            SelectedEquipment = "All";
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        _searchCts?.Cancel();
        SearchQuery = string.Empty;
        _lastValidMuscle = "All";
        _lastValidEquipment = "All";
        SelectedMuscleGroup = "All";
        SelectedEquipment = "All";
    }

    [RelayCommand]
    private async Task NavigateToDetail(Exercise exercise)
    {
        await Shell.Current.GoToAsync(
            $"{nameof(Views.Exercises.ExerciseDetailPage)}?ExerciseId={exercise.Id}");
    }

    [RelayCommand]
    private async Task NavigateToCreate()
    {
        await Shell.Current.GoToAsync(nameof(Views.Exercises.CreateExercisePage));
    }
}

