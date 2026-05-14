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
}
