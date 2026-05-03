using Golyath.Application.Services;

namespace Golyath.UI.Views.Onboarding;

public partial class OnboardingCompletePage : ContentPage
{
    private readonly IServiceProvider _services;

    public OnboardingCompletePage(IUserService userService, IServiceProvider services)
    {
        InitializeComponent();
        _services = services;

        // Personalise the welcome label asynchronously
        _ = SetWelcomeLabelAsync(userService);
    }

    protected override bool OnBackButtonPressed() => true; // Prevent navigating back

    private async Task SetWelcomeLabelAsync(IUserService userService)
    {
        var user = await userService.GetCurrentUserAsync();
        if (user is not null)
            WelcomeLabel.Text = $"Welcome, {user.Nickname}!";
    }

    private void OnStartTrainingClicked(object sender, EventArgs e)
    {
        Microsoft.Maui.Controls.Application.Current!.MainPage = _services.GetRequiredService<AppShell>();
    }
}
