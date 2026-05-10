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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Reload if WorkoutId is already set but data hasn't loaded yet
        if (_viewModel.WorkoutId > 0 && _viewModel.Detail is null && !_viewModel.IsBusy)
            _ = _viewModel.LoadAsync();
    }
}
