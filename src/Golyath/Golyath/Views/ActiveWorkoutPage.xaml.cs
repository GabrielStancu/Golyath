using Golyath.ViewModels;

namespace Golyath.Views;

public partial class ActiveWorkoutPage : ContentPage
{
    private readonly ActiveWorkoutViewModel _vm;

    public ActiveWorkoutPage(ActiveWorkoutViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}
