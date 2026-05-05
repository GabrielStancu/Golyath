using Golyath.UI.ViewModels.Exercises;

namespace Golyath.UI.Views.Exercises;

public partial class ExerciseDetailPage : ContentPage
{
    private readonly ExerciseDetailViewModel _viewModel;

    public ExerciseDetailPage(ExerciseDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.StartCarousel();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopCarousel();
    }
}
