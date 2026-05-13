using Golyath.UI.ViewModels.Workout;
using Golyath.UI.Views.Workout;

namespace Golyath.UI.Views.Workout;

public partial class ActiveWorkoutPage : ContentPage
{
    private readonly ActiveWorkoutViewModel _viewModel;
    private readonly IServiceProvider _services;
    private bool _initialized;

    public ActiveWorkoutPage(ActiveWorkoutViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _services = services;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RegisterMessenger();
        _viewModel.WorkoutCompleted += OnWorkoutCompleted;
        _viewModel.AddExerciseRequested += OnAddExerciseRequested;

        if (!_initialized)
        {
            _initialized = true;
            _ = _viewModel.InitializeAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.WorkoutCompleted -= OnWorkoutCompleted;
        _viewModel.AddExerciseRequested -= OnAddExerciseRequested;
        // Keep messenger registered while page is on the stack so exercise picker can
        // deliver its message when returning from ExercisePickerPage.
    }

    private async void OnAddExerciseRequested(object? sender, EventArgs e)
    {
        var pickerPage = _services.GetRequiredService<ExercisePickerPage>();
        await Navigation.PushAsync(pickerPage);
    }

    private async void OnWorkoutCompleted(object? sender, EventArgs e)
    {
        _viewModel.UnregisterMessenger();
        await Navigation.PopAsync();
    }

    private async void OnBackClicked(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
