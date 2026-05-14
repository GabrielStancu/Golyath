using Golyath.UI.Controls;
using Golyath.UI.ViewModels.Exercises;

namespace Golyath.UI.Views.Exercises;

public partial class CreateExercisePage : ContentPage
{
    private readonly CreateExerciseViewModel _vm;

    public CreateExercisePage(CreateExerciseViewModel viewModel)
    {
        InitializeComponent();
        _vm = viewModel;
        BindingContext = viewModel;
    }

    private async void OnMuscleGroupTapped(object? sender, TappedEventArgs e)
    {
        var popup = new SelectionPopup("Primary Muscle", _vm.MuscleGroupOptions, _vm.SelectedMuscleGroupDisplay);
        var result = await popup.ShowAsync(this);
        if (result is string selected)
        {
            var index = _vm.MuscleGroupOptions.IndexOf(selected);
            if (index >= 0) _vm.SelectedMuscleGroupIndex = index;
        }
    }

    private async void OnEquipmentTapped(object? sender, TappedEventArgs e)
    {
        var popup = new SelectionPopup("Equipment", _vm.EquipmentOptions, _vm.SelectedEquipmentDisplay);
        var result = await popup.ShowAsync(this);
        if (result is string selected)
        {
            var index = _vm.EquipmentOptions.IndexOf(selected);
            if (index >= 0) _vm.SelectedEquipmentIndex = index;
        }
    }

    private async void OnMovementTypeTapped(object? sender, TappedEventArgs e)
    {
        var popup = new SelectionPopup("Movement Type", _vm.MovementTypeOptions, _vm.SelectedMovementTypeDisplay);
        var result = await popup.ShowAsync(this);
        if (result is string selected)
        {
            var index = _vm.MovementTypeOptions.IndexOf(selected);
            if (index >= 0) _vm.SelectedMovementTypeIndex = index;
        }
    }
}
