using Golyath.UI.Controls;
using Golyath.UI.ViewModels.Exercises;

namespace Golyath.UI.Views.Exercises;

public partial class ExerciseLibraryPage : ContentPage
{
    private readonly ExerciseLibraryViewModel _viewModel;

    public ExerciseLibraryPage(ExerciseLibraryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Page fade-in transition
        this.Opacity = 0;
        _ = this.FadeTo(1, 300, Easing.CubicOut);

        await _viewModel.InitializeAsync();
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
