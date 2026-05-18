using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.Core.Entities;
using Golyath.UI.Controls;
using Golyath.UI.ViewModels.Workout;

namespace Golyath.UI.ViewModels.History;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IWorkoutHistoryService _historyService;

    public static readonly string[] PeriodOptions =
        ["All time", "This week", "This month", "Last 3 months"];

    // ── Filter state ─────────────────────────────────────────────────────────
    [ObservableProperty] private string _selectedPeriod = "All time";
    [ObservableProperty] private Tag? _selectedTag;
    [ObservableProperty] private bool _hasActiveFilters;

    // ── Data ─────────────────────────────────────────────────────────────────
    [ObservableProperty] private List<WorkoutHistorySummaryDto> _workouts = [];
    [ObservableProperty] private List<Tag> _availableTags = [];
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isEmpty;

    public HistoryViewModel(IWorkoutHistoryService historyService)
    {
        _historyService = historyService;
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var tagsTask = _historyService.GetAllTagsAsync();

            var (from, to) = ResolveDateRange(SelectedPeriod);
            var historyTask = _historyService.GetHistoryAsync(from, to, SelectedTag?.Id);

            await Task.WhenAll(tagsTask, historyTask);

            AvailableTags = [.. tagsTask.Result];
            Workouts = [.. historyTask.Result];
            IsEmpty = Workouts.Count == 0;
            UpdateHasActiveFilters();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ClearFiltersAsync()
    {
        SelectedPeriod = "All time";
        SelectedTag = null;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task OpenWorkoutAsync(WorkoutHistorySummaryDto workout)
    {
        await Shell.Current.GoToAsync($"WorkoutDetail?workoutId={workout.Id}");
    }

    [RelayCommand]
    private async Task DeleteWorkoutAsync(WorkoutHistorySummaryDto workout)
    {
        var popup = new ConfirmPopup(
            "Delete Workout",
            "Permanently delete this workout and all its data?",
            "Delete",
            "Cancel",
            isDestructive: true);
        var result = await popup.ShowAsync();
        if (result is not true) return;

        IsBusy = true;
        try
        {
            await _historyService.DeleteWorkoutAsync(workout.Id);
            WeakReferenceMessenger.Default.Send(new WorkoutChangedMessage(workout.Id));
            var list = new List<WorkoutHistorySummaryDto>(Workouts);
            list.RemoveAll(w => w.Id == workout.Id);
            Workouts = list;
            IsEmpty = Workouts.Count == 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateHasActiveFilters()
    {
        HasActiveFilters = SelectedPeriod != "All time" || SelectedTag is not null;
    }

    private static (DateTime? from, DateTime? to) ResolveDateRange(string period) => period switch
    {
        "This week" => (DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek), null),
        "This month" => (new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc), null),
        "Last 3 months" => (DateTime.UtcNow.AddMonths(-3), null),
        _ => (null, null)
    };
}
