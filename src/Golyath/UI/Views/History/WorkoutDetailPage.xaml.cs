using Golyath.UI.ViewModels.History;

namespace Golyath.UI.Views.History;

public partial class WorkoutDetailPage : ContentPage
{
    private readonly WorkoutDetailViewModel _viewModel;

    public WorkoutDetailPage(WorkoutDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}
