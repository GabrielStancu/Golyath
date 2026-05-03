namespace Golyath.Application.Services;

public interface IThemeService
{
    AppTheme GetPreferredTheme();
    void ApplyTheme(AppTheme theme);
    void ApplyPreferredTheme();
}
