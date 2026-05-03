using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Golyath.UI.ViewModels.Onboarding;

public partial class WelcomeViewModel : ObservableObject
{
    public event EventHandler? NewProfileRequested;
    public event EventHandler? RestoreBackupRequested;

    [RelayCommand]
    private void NewProfile() => NewProfileRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void RestoreBackup() => RestoreBackupRequested?.Invoke(this, EventArgs.Empty);
}
