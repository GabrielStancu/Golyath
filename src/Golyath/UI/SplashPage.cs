using Golyath.Application.Services;
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

        var userService = _services.GetRequiredService<IUserService>();
        var hasUser = await userService.HasCompletedOnboardingAsync();

        Microsoft.Maui.Controls.Application.Current!.MainPage = hasUser
            ? _services.GetRequiredService<AppShell>()
            : new NavigationPage(_services.GetRequiredService<WelcomePage>());
    }
}
