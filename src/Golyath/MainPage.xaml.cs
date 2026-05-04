using Golyath.UI.Views.Profile;
using Golyath.UI.Views.Workout;

namespace Golyath;

public partial class MainPage : ContentPage
{
    private readonly IServiceProvider _services;

    public MainPage(IServiceProvider services)
    {
        InitializeComponent();
        _services = services;
    }

    private async void OnProfileClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
    }

    private async void OnStartWorkoutClicked(object? sender, EventArgs e)
    {
        var page = _services.GetRequiredService<ActiveWorkoutPage>();
        await Navigation.PushAsync(page);
    }
}
