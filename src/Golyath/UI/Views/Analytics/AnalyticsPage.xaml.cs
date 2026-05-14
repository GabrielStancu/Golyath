using Golyath.UI.ViewModels.Analytics;

namespace Golyath.UI.Views.Analytics;

public partial class AnalyticsPage : ContentPage
{
    private readonly AnalyticsViewModel _vm;

    public AnalyticsPage(AnalyticsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Page fade-in transition
        this.Opacity = 0;
        _ = this.FadeTo(1, 300, Easing.CubicOut);

        await _vm.LoadAsync();
    }

    private async void OnSuggestionsTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("SuggestionsPage");
    }
}
