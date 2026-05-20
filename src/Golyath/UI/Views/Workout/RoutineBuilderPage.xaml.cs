using Golyath.Core.Enums;
using Golyath.UI.ViewModels.Workout;

namespace Golyath.UI.Views.Workout;

public partial class RoutineBuilderPage : ContentPage
{
    private readonly RoutineBuilderViewModel _viewModel;
    private readonly IServiceProvider _services;
    private bool _initialized;

    public RoutineBuilderPage(RoutineBuilderViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _services = services;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RegisterMessenger();
        _viewModel.AddExerciseRequested += OnAddExerciseRequested;
        _viewModel.Exercises.CollectionChanged += OnExercisesChanged;
        _viewModel.MuscleSelectionChanged += OnMuscleSelectionChanged;
        UpdateEmptyState();

        if (!_initialized)
        {
            _initialized = true;
            _ = InitializeAsync();
        }
        else
        {
            RefreshMuscleChipStyles();
        }
    }

    private async Task InitializeAsync()
    {
        await _viewModel.LoadAsync();
        RefreshMuscleChipStyles();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.AddExerciseRequested -= OnAddExerciseRequested;
        _viewModel.Exercises.CollectionChanged -= OnExercisesChanged;
        _viewModel.MuscleSelectionChanged -= OnMuscleSelectionChanged;
        // Keep messenger registered while page is on the stack so exercise picker can
        // deliver its message when returning from ExercisePickerPage.
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        _viewModel.UnregisterMessenger();
        await Shell.Current.GoToAsync("..");
    }

    private void OnCancelClicked(object? sender, TappedEventArgs e)
    {
        _viewModel.UnregisterMessenger();
        _ = Shell.Current.GoToAsync("..");
    }

    private async void OnAddExerciseRequested(object? sender, EventArgs e)
    {
        var pickerPage = _services.GetRequiredService<ExercisePickerPage>();
        await Navigation.PushAsync(pickerPage);
    }

    private void OnExercisesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateEmptyState();
    }

    private void OnMuscleSelectionChanged(object? sender, EventArgs e)
    {
        RefreshMuscleChipStyles();
    }

    private void UpdateEmptyState()
    {
        EmptyState.IsVisible = _viewModel.Exercises.Count == 0;
    }

    private void RefreshMuscleChipStyles()
    {
        var goldColor = Color.FromArgb("#FFD700");
        var darkColor = Color.FromArgb("#111111");

        foreach (var child in MuscleChipsLayout.Children)
        {
            if (child is Border border && border.Content is Label label)
            {
                var chip = _viewModel.MuscleChips.FirstOrDefault(c => c.Name == label.Text);
                bool selected = chip?.IsSelected ?? false;
                border.BackgroundColor = selected ? goldColor : Colors.Transparent;
                label.TextColor = selected ? darkColor : goldColor;
            }
        }
    }

    private Layout GetChipContainer()
    {
        return MuscleChipsLayout;
    }
}
