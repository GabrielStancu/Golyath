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

        // Page fade-in transition
        this.Opacity = 0;
        _ = this.FadeTo(1, 300, Easing.CubicOut);

        await _viewModel.LoadAsync();
    }

    private async void OnPeriodFilterTapped(object? sender, TappedEventArgs e)
    {
        var popup = new SelectionPopup("Period", HistoryViewModel.PeriodOptions, _viewModel.SelectedPeriod);
        var result = await popup.ShowAsync(this);
        if (result is string selected)
            _viewModel.SelectedPeriod = selected;
    }

    private async void OnTagFilterTapped(object? sender, TappedEventArgs e)
    {
        var names = new List<string> { "All tags" };
        names.AddRange(_viewModel.AvailableTags.Select(t => t.Name));
        var popup = new SelectionPopup("Tag", names, _viewModel.SelectedTag?.Name ?? "All tags");
        var result = await popup.ShowAsync(this);
        if (result is string selected)
            _viewModel.SelectedTag = selected == "All tags" ? null : _viewModel.AvailableTags.FirstOrDefault(t => t.Name == selected);
    }
}
