using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Golyath.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private const string UnitsKey = "PreferKg";

    [ObservableProperty]
    private bool _useKilograms;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    public SettingsViewModel()
    {
        Title = "Settings";
        UseKilograms = Preferences.Get(UnitsKey, true);
        AppVersion = AppInfo.VersionString;
    }

    partial void OnUseKilogramsChanged(bool value)
    {
        Preferences.Set(UnitsKey, value);
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
