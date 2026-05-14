using Golyath.UI.Controls;
using Golyath.UI.ViewModels.Settings;

namespace Golyath.UI.Views.Settings;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _vm;

    public SettingsPage(SettingsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.InitializeAsync();
    }

    private async void OnRestTimerTapped(object? sender, TappedEventArgs e)
    {
        var popup = new SelectionPopup("Rest Timer", _vm.RestTimerLabels, _vm.SelectedRestTimerLabel);
        var result = await popup.ShowAsync(this);
        if (result is string selected)
        {
            var index = _vm.RestTimerLabels.ToList().IndexOf(selected);
            if (index >= 0)
                _vm.SelectedRestTimerIndex = index;
        }
    }
}
