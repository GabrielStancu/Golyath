namespace Golyath.Application.Services;

public sealed class SettingsService : ISettingsService
{
    private const string RestTimerKey = "DefaultRestSeconds";
    public const int DefaultRestSeconds = 90;

    public int GetDefaultRestSeconds()
        => Preferences.Default.Get(RestTimerKey, DefaultRestSeconds);

    public void SetDefaultRestSeconds(int seconds)
        => Preferences.Default.Set(RestTimerKey, seconds);
}
