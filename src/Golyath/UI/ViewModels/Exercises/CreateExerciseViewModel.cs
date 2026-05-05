using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Core.Enums;

namespace Golyath.UI.ViewModels.Exercises;

public partial class CreateExerciseViewModel : ObservableObject
{
    private readonly IExerciseRepository _exerciseRepository;

    // ── Form fields ─────────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _selectedMuscleGroupIndex = 0;

    [ObservableProperty]
    private int _selectedEquipmentIndex = 0;

    [ObservableProperty]
    private int _selectedMovementTypeIndex = 0;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    // ── Picker options ───────────────────────────────────────────────────────────

    public List<string> MuscleGroupOptions { get; } = Enum.GetNames<MuscleGroup>().ToList();
    public List<string> EquipmentOptions { get; } = Enum.GetNames<EquipmentType>().ToList();
    public List<string> MovementTypeOptions { get; } = Enum.GetNames<MovementType>().ToList();

    public CreateExerciseViewModel(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    private bool CanSave => !string.IsNullOrWhiteSpace(Name);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        IsBusy = true;
        try
        {
            var exercise = new Exercise
            {
                Name = Name.Trim(),
                PrimaryMuscle = Enum.Parse<MuscleGroup>(MuscleGroupOptions[SelectedMuscleGroupIndex]),
                Equipment = Enum.Parse<EquipmentType>(EquipmentOptions[SelectedEquipmentIndex]),
                MovementType = Enum.Parse<MovementType>(MovementTypeOptions[SelectedMovementTypeIndex]),
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                IsCustom = true,
            };

            await _exerciseRepository.InsertAsync(exercise);
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Cancel() =>
        await Shell.Current.GoToAsync("..");
}
