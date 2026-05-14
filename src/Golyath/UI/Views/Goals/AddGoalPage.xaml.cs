using Golyath.UI.Controls;
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

    private async void OnExercisePickerTapped(object? sender, TappedEventArgs e)
    {
        if (_vm.Exercises.Count == 0) return;

        var names = _vm.Exercises.Select(ex => ex.Name).ToList();
        var popup = new SelectionPopup("Select Exercise", names, _vm.SelectedExercise?.Name);
        var result = await popup.ShowAsync(this);

        if (result is string selected)
            _vm.SelectedExercise = _vm.Exercises.FirstOrDefault(ex => ex.Name == selected);
    }
}
