namespace Golyath.Application.Services;

public interface ISettingsService
{
    int GetDefaultRestSeconds();
    void SetDefaultRestSeconds(int seconds);
}
