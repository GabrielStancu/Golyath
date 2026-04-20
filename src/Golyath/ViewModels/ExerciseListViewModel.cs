using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Models;
using Golyath.Services;
using System.Collections.ObjectModel;

namespace Golyath.ViewModels;

/// <summary>Represents a muscle group filter chip with an observable IsSelected state.</summary>
public partial class MuscleGroupChipItem : ObservableObject
{
    public MuscleGroup? Group { get; init; } // null = "All"
    public string DisplayName { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}

public partial class ExerciseListViewModel : BaseViewModel
{
    private readonly IExerciseService _exerciseService;

    private List<Exercise> _allExercises = [];
    private List<MuscleGroup> _allMuscleGroups = [];

    [ObservableProperty]
    private ObservableCollection<Exercise> _exercises = [];

    [ObservableProperty]
    private ObservableCollection<MuscleGroupChipItem> _muscleGroupChips = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private MuscleGroup? _selectedMuscleGroup;

    public ExerciseListViewModel(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
        Title = "Exercises";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            _allExercises = await _exerciseService.GetAllExercisesAsync();
            _allMuscleGroups = await _exerciseService.GetMuscleGroupsAsync();

            // Resolve display names for badges
            var mgMap = _allMuscleGroups.ToDictionary(m => m.Id, m => m.DisplayName);
            foreach (var ex in _allExercises)
                ex.MuscleGroupName = mgMap.GetValueOrDefault(ex.PrimaryMuscleGroupId, string.Empty);

            // Build filter chips: "All" first, then one per muscle group
            var chips = new List<MuscleGroupChipItem>
            {
                new() { Group = null, DisplayName = "All", IsSelected = true }
            };
            chips.AddRange(_allMuscleGroups.Select(m =>
                new MuscleGroupChipItem { Group = m, DisplayName = m.DisplayName }));
            MuscleGroupChips = new ObservableCollection<MuscleGroupChipItem>(chips);

            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnSelectedMuscleGroupChanged(MuscleGroup? value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = _allExercises.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(e =>
                e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        if (SelectedMuscleGroup is not null)
            filtered = filtered.Where(e =>
                e.PrimaryMuscleGroupId == SelectedMuscleGroup.Id);

        Exercises = new ObservableCollection<Exercise>(filtered.OrderBy(e => e.Name));
    }

    [RelayCommand]
    private void SelectChip(MuscleGroupChipItem chip)
    {
        foreach (var c in MuscleGroupChips)
            c.IsSelected = false;
        chip.IsSelected = true;
        SelectedMuscleGroup = chip.Group;
    }

    [RelayCommand]
    private void ClearFilter()
    {
        var allChip = MuscleGroupChips.FirstOrDefault(c => c.Group is null);
        if (allChip is not null) SelectChip(allChip);
        SearchText = string.Empty;
    }

    [RelayCommand]
    private async Task SelectExerciseAsync(Exercise exercise)
    {
        await Shell.Current.GoToAsync($"exercise-detail?exerciseId={exercise.Id}");
    }
}
