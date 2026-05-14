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
}
