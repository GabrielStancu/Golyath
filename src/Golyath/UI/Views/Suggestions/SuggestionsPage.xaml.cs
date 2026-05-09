using Golyath.UI.ViewModels.Suggestions;

namespace Golyath.UI.Views.Suggestions;

public partial class SuggestionsPage : ContentPage
{
    private readonly SuggestionsViewModel _vm;

    public SuggestionsPage(SuggestionsViewModel vm)
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
