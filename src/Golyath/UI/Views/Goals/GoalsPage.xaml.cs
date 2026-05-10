using Golyath.UI.ViewModels.Goals;

namespace Golyath.UI.Views.Goals;

public partial class GoalsPage : ContentPage
{
    private readonly GoalsViewModel _vm;

    public GoalsPage(GoalsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
