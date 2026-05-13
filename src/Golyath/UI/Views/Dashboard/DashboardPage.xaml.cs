using Golyath.UI.ViewModels.Dashboard;

namespace Golyath.UI.Views.Dashboard;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
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

        await _viewModel.LoadAsync();
    }

    private async void OnSettingsClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("SettingsPage");
    }
}
