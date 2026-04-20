using Golyath.ViewModels;

namespace Golyath.Views;

public partial class ExerciseListPage : ContentPage
{
    private readonly ExerciseListViewModel _vm;

    public ExerciseListPage(ExerciseListViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Exercises.Count == 0)
            await _vm.LoadCommand.ExecuteAsync(null);
    }
}
