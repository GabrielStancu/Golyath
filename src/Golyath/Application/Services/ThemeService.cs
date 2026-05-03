using MauiApplication = Microsoft.Maui.Controls.Application;

namespace Golyath.Application.Services;

public sealed class ThemeService : IThemeService
{
    private const string ThemeKey = "AppTheme";

    public AppTheme GetPreferredTheme()
    {
        var saved = Preferences.Default.Get(ThemeKey, "System");
        return saved switch
        {
            "Light" => AppTheme.Light,
            "Dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }

    public void ApplyTheme(AppTheme theme)
    {
        var value = theme switch
        {
            AppTheme.Light => "Light",
            AppTheme.Dark => "Dark",
            _ => "System"
        };
        Preferences.Default.Set(ThemeKey, value);
        if (MauiApplication.Current is not null)
            MauiApplication.Current.UserAppTheme = theme;
    }

    public void ApplyPreferredTheme() => ApplyTheme(GetPreferredTheme());
}
