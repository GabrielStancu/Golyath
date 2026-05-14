using Golyath.UI.Controls;
using Golyath.UI.ViewModels.Onboarding;

namespace Golyath.UI.Views.Onboarding;

public partial class ProfileSetupPage : ContentPage
{
    private readonly ProfileSetupViewModel _viewModel;
    private readonly IServiceProvider _services;

    public ProfileSetupPage(ProfileSetupViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _services = services;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ContinueRequested += OnContinueRequested;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.ContinueRequested -= OnContinueRequested;
    }

    private async void OnContinueRequested(object? sender, EventArgs e)
    {
        var page = _services.GetRequiredService<GoalSetupPage>();
        await Navigation.PushAsync(page);
    }

    private async void OnGenderTapped(object? sender, TappedEventArgs e)
    {
        var popup = new SelectionPopup("Gender", _viewModel.GenderOptions, _viewModel.SelectedGenderDisplay);
        var result = await popup.ShowAsync(this);
        if (result is string selected)
            _viewModel.SelectedGenderDisplay = selected;
    }
}
