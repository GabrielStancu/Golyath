using Golyath.Application.Localization;
using Golyath.Application.Services;
using Golyath.Core.Abstractions;
using Golyath.UI.Views.Onboarding;

namespace Golyath.UI;

/// <summary>
/// Minimal startup page. Applies the saved theme and routes to onboarding or the main shell
/// based on whether a user profile exists.
/// </summary>
internal sealed class SplashPage : ContentPage
{
    private readonly IServiceProvider _services;

    public SplashPage(IServiceProvider services)
    {
        _services = services;
        BackgroundColor = Color.FromArgb("#111111");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var themeService = _services.GetRequiredService<IThemeService>();
        themeService.ApplyPreferredTheme();

        var seeder = _services.GetRequiredService<IExerciseSeederService>();
        await seeder.SeedAsync();

        var userService = _services.GetRequiredService<IUserService>();

        // Apply the saved language before any page is constructed so all
        // x:Static and indexer bindings resolve in the correct culture.
        var savedUser = await userService.GetCurrentUserAsync();
        if (savedUser is not null)
            LocalizationManager.Instance.SetLanguage(savedUser.Language);

        var hasUser = await userService.HasCompletedOnboardingAsync();

        Microsoft.Maui.Controls.Application.Current!.MainPage = hasUser
            ? _services.GetRequiredService<AppShell>()
            : new NavigationPage(_services.GetRequiredService<WelcomePage>());
    }
}
