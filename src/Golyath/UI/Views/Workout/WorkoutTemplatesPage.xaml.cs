using Golyath.UI.ViewModels.Workout;

namespace Golyath.UI.Views.Workout;

public partial class WorkoutTemplatesPage : ContentPage
{
    private readonly WorkoutTemplatesViewModel _viewModel;

    public WorkoutTemplatesPage(WorkoutTemplatesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RegisterMessenger();
        await _viewModel.LoadAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.UnregisterMessenger();
    }
}
