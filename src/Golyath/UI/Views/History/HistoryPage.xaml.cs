using Golyath.UI.ViewModels.History;

namespace Golyath.UI.Views.History;

public partial class HistoryPage : ContentPage
{
    private readonly HistoryViewModel _viewModel;

    public HistoryPage(HistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Page fade-in transition
        this.Opacity = 0;
        _ = this.FadeTo(1, 300, Easing.CubicOut);

        await _viewModel.LoadAsync();
    }
}
