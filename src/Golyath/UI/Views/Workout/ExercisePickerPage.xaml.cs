using Golyath.UI.Controls;
using Golyath.UI.ViewModels.Workout;

namespace Golyath.UI.Views.Workout;

public partial class ExercisePickerPage : ContentPage
{
    private readonly ExercisePickerViewModel _viewModel;

    public ExercisePickerPage(ExercisePickerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _viewModel.InitializeAsync();
    }

    private async void OnBackClicked(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnMuscleFilterTapped(object? sender, TappedEventArgs e)
    {
        var popup = new SelectionPopup("Muscle Group", _viewModel.MuscleGroupOptions, _viewModel.SelectedMuscleGroup);
        var result = await popup.ShowAsync(this);
        if (result is string selected)
            _viewModel.SelectedMuscleGroup = selected;
    }

    private async void OnEquipmentFilterTapped(object? sender, TappedEventArgs e)
    {
        var popup = new SelectionPopup("Equipment", _viewModel.EquipmentOptions, _viewModel.SelectedEquipment);
        var result = await popup.ShowAsync(this);
        if (result is string selected)
            _viewModel.SelectedEquipment = selected;
    }
}
