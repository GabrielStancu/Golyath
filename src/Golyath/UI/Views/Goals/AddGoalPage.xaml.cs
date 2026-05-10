using Golyath.UI.ViewModels.Goals;

namespace Golyath.UI.Views.Goals;

public partial class AddGoalPage : ContentPage
{
    private readonly AddGoalViewModel _vm;

    public AddGoalPage(AddGoalViewModel vm)
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
