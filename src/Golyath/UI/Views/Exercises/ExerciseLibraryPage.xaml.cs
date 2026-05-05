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
        await _viewModel.InitializeAsync();
    }
}
