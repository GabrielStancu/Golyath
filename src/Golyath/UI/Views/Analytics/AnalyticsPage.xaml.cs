using Golyath.UI.Controls;
using Golyath.UI.ViewModels.Analytics;

namespace Golyath.UI.Views.Analytics;

public partial class AnalyticsPage : ContentPage
{
    private readonly AnalyticsViewModel _vm;

    public AnalyticsPage(AnalyticsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        this.Opacity = 0;
        _ = this.FadeTo(1, 300, Easing.CubicOut);

        await _vm.LoadAsync();
    }

    private async void OnExercisePickerTapped(object? sender, TappedEventArgs e)
    {
        if (_vm.ExerciseOptions.Count == 0) return;

        var names = _vm.ExerciseOptions.Select(ex => ex.Name).ToList();
        var popup = new SelectionPopup("Select Exercise", names, _vm.SelectedExercise?.Name);
        var result = await popup.ShowAsync(this);

        if (result is string selected)
            _vm.SelectedExercise = _vm.ExerciseOptions.FirstOrDefault(ex => ex.Name == selected);
    }
}

