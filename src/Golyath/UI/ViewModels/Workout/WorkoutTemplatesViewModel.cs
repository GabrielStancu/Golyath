using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.UI.Views.Workout;

namespace Golyath.UI.ViewModels.Workout;

public partial class WorkoutTemplatesViewModel : ObservableObject
{
    private readonly IWorkoutHistoryService _historyService;

    public ObservableCollection<WorkoutHistorySummaryDto> RecentWorkouts { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasWorkouts;

    public bool HasNoWorkouts => !HasWorkouts;

    public WorkoutTemplatesViewModel(IWorkoutHistoryService historyService)
    {
        _historyService = historyService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var history = await _historyService.GetHistoryAsync();
            RecentWorkouts.Clear();

            // Show unique workout names as "templates" from past workouts
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var w in history)
            {
                var name = w.DisplayName ?? "Workout";
                if (seen.Add(name))
                    RecentWorkouts.Add(w);

                if (seen.Count >= 20) break;
            }

            HasWorkouts = RecentWorkouts.Count > 0;
            OnPropertyChanged(nameof(HasNoWorkouts));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartFreeWorkout()
    {
        await Shell.Current.GoToAsync(nameof(ActiveWorkoutPage));
    }

    [RelayCommand]
    private async Task RepeatWorkout(WorkoutHistorySummaryDto workout)
    {
        // Navigate to active workout page — the template concept can be extended later
        await Shell.Current.GoToAsync(nameof(ActiveWorkoutPage));
    }
}
