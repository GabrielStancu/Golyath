using Golyath.ViewModels;

namespace Golyath.Views;

public partial class ExerciseDetailPage : ContentPage
{
    public ExerciseDetailPage(ExerciseDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ExerciseDetailViewModel vm)
            vm.StartAnimation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is ExerciseDetailViewModel vm)
            vm.StopAnimation();
    }

    private async void OnBackTapped(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}
