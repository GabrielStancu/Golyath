using Golyath.UI.Controls;
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
        this.Opacity = 0;
        _ = this.FadeTo(1, 300, Easing.CubicOut);
        await _viewModel.LoadAsync();
    }

    private async void OnFilterTapped(object? sender, TappedEventArgs e)
    {
        var popup = new SelectionPopup(
            "Filter by period",
            HistoryViewModel.PeriodOptions,
            _viewModel.SelectedPeriod);

        var result = await popup.ShowAsync(this);
        if (result is not string selected) return;

        _viewModel.SelectedPeriod = selected;
        _viewModel.ApplyFilterCommand.Execute(null);
    }
}
