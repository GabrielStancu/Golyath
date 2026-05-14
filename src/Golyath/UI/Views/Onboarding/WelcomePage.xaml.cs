using Golyath.Application.Services;
using Golyath.UI.Controls;
using Golyath.UI.ViewModels.Onboarding;

namespace Golyath.UI.Views.Onboarding;

public partial class WelcomePage : ContentPage
{
    private readonly WelcomeViewModel _viewModel;
    private readonly IServiceProvider _services;
    private readonly IDataPortabilityService _dataPortabilityService;

    public WelcomePage(WelcomeViewModel viewModel, IServiceProvider services,
        IDataPortabilityService dataPortabilityService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _services = services;
        _dataPortabilityService = dataPortabilityService;
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
        var fileResult = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select Golyath backup",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".json"] },
                { DevicePlatform.Android, ["*/*"] },
                { DevicePlatform.iOS, ["public.json", "public.text"] },
                { DevicePlatform.MacCatalyst, ["public.json", "public.text"] }
            })
        });

        if (fileResult is null) return;

        try
        {
            // Pass the raw stream directly — avoids encoding issues from StreamReader
            // or content:// provider wrappers (Google Drive, file managers, etc.).
            using var stream = await fileResult.OpenReadAsync();
            var result = await _dataPortabilityService.ImportFromStreamAsync(stream);

            if (!result.Success)
            {
                var errorPopup = new ConfirmPopup("Restore Failed", result.Message, "OK", "Close");
                await errorPopup.ShowAsync(this);
                return;
            }

            // Navigate to the main shell — data is now restored
            Microsoft.Maui.Controls.Application.Current!.MainPage = _services.GetRequiredService<AppShell>();
        }
        catch (Exception ex)
        {
            var errorPopup = new ConfirmPopup("Restore Failed", ex.Message, "OK", "Close");
            await errorPopup.ShowAsync(this);
        }
    }
}
