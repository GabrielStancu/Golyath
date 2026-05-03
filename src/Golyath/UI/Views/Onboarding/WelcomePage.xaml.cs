using Golyath.UI.ViewModels.Onboarding;

namespace Golyath.UI.Views.Onboarding;

public partial class WelcomePage : ContentPage
{
    private readonly WelcomeViewModel _viewModel;
    private readonly IServiceProvider _services;

    public WelcomePage(WelcomeViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _services = services;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.NewProfileRequested += OnNewProfileRequested;
        _viewModel.RestoreBackupRequested += OnRestoreBackupRequested;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.NewProfileRequested -= OnNewProfileRequested;
        _viewModel.RestoreBackupRequested -= OnRestoreBackupRequested;
    }

    private async void OnNewProfileRequested(object? sender, EventArgs e)
    {
        var page = _services.GetRequiredService<ProfileSetupPage>();
        await Navigation.PushAsync(page);
    }

    private async void OnRestoreBackupRequested(object? sender, EventArgs e)
    {
        await DisplayAlert(
            "Coming Soon",
            "Backup restore will be available once you've started using the app and created your first export.",
            "OK");
    }
}
