using Golyath.UI.ViewModels.Profile;

namespace Golyath.UI.Views.Profile;

public partial class EditProfilePage : ContentPage
{
    private readonly EditProfileViewModel _viewModel;

    public EditProfilePage(EditProfileViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.SaveCompleted += OnSaveCompleted;
        await _viewModel.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.SaveCompleted -= OnSaveCompleted;
    }

    private async void OnSaveCompleted(object? sender, EventArgs e)
    {
        await Task.Delay(800); // Brief display of "Profile saved!"
        await Shell.Current.GoToAsync("..");
    }
}
