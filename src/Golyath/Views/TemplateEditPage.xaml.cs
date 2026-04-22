namespace Golyath.Views;

public partial class TemplateEditPage : ContentPage
{
    public TemplateEditPage(ViewModels.TemplateEditViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
