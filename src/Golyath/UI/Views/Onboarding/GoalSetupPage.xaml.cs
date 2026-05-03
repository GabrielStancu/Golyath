using Golyath.UI.ViewModels.Onboarding;

namespace Golyath.UI.Views.Onboarding;

public partial class GoalSetupPage : ContentPage
{
    private readonly GoalSetupViewModel _viewModel;
    private readonly IServiceProvider _services;

    public GoalSetupPage(GoalSetupViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _services = services;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.CompletedRequested += OnCompletedRequested;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.CompletedRequested -= OnCompletedRequested;
    }

    private async void OnCompletedRequested(object? sender, EventArgs e)
    {
        var page = _services.GetRequiredService<OnboardingCompletePage>();
        await Navigation.PushAsync(page);
    }
}
