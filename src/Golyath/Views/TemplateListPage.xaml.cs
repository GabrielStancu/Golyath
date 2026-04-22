using Golyath.ViewModels;

namespace Golyath.Views;

public partial class TemplateListPage : ContentPage
{
    private readonly TemplateListViewModel _vm;

    public TemplateListPage(TemplateListViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = _vm.LoadAsync();
    }
}
