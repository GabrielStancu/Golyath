using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Models;
using Golyath.Services;
using System.Collections.ObjectModel;

namespace Golyath.ViewModels;

[QueryProperty(nameof(ExerciseId), "exerciseId")]
public partial class ExerciseDetailViewModel : BaseViewModel
{
    private readonly IExerciseService _exerciseService;

    [ObservableProperty]
    private int _exerciseId;

    [ObservableProperty]
    private Exercise? _exercise;

    [ObservableProperty]
    private string _primaryMuscleName = string.Empty;

    [ObservableProperty]
    private string _equipmentName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _secondaryMuscleNames = [];

    public bool HasSecondaryMuscles => SecondaryMuscleNames.Count > 0;
    public bool HasDescription => !string.IsNullOrWhiteSpace(Exercise?.Description);

    public ExerciseDetailViewModel(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    partial void OnExerciseIdChanged(int value)
    {
        if (value > 0)
            LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy || ExerciseId <= 0) return;
        IsBusy = true;
        try
        {
            Exercise = await _exerciseService.GetExerciseByIdAsync(ExerciseId);
            if (Exercise is null) return;

            Title = Exercise.Name;

            var muscleGroups = await _exerciseService.GetMuscleGroupsAsync();
            var equipment = await _exerciseService.GetEquipmentAsync();
            var links = await _exerciseService.GetMuscleLinksForExerciseAsync(ExerciseId);

            var primary = muscleGroups.FirstOrDefault(m => m.Id == Exercise.PrimaryMuscleGroupId);
            PrimaryMuscleName = primary?.DisplayName ?? "Unknown";

            var eq = equipment.FirstOrDefault(e => e.Id == Exercise.EquipmentId);
            EquipmentName = eq?.Name ?? "Unknown";

            var secondaryIds = links
                .Where(l => l.Role == MuscleRole.Secondary)
                .Select(l => l.MuscleGroupId);

            var secondaryNames = muscleGroups
                .Where(m => secondaryIds.Contains(m.Id))
                .Select(m => m.DisplayName);

            SecondaryMuscleNames = new ObservableCollection<string>(secondaryNames);
            OnPropertyChanged(nameof(HasSecondaryMuscles));
            OnPropertyChanged(nameof(HasDescription));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
