using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.Core.Entities;
using Golyath.UI.ViewModels.Workout;

namespace Golyath.UI.ViewModels.History;

public partial class HistoryViewModel : ObservableObject
{
    private readonly IWorkoutHistoryService _historyService;

    public static readonly string[] PeriodOptions =
        ["All time", "This week", "This month", "Last 3 months"];

    // ── All workouts (unfiltered, used for chart + monthly stats) ─────────────
    private List<WorkoutHistorySummaryDto> _allWorkouts = [];

    // ── Filter state ─────────────────────────────────────────────────────────
    [ObservableProperty] private string _selectedPeriod = "All time";
    [ObservableProperty] private Tag? _selectedTag;

    // ── Display data ─────────────────────────────────────────────────────────
    [ObservableProperty] private List<WorkoutHistorySummaryDto> _workouts = [];
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isEmpty;

    // ── Monthly stats ─────────────────────────────────────────────────────────
    [ObservableProperty] private string _monthSessionCount = "0";
    [ObservableProperty] private string _monthVolumeText = "0 kg";
    [ObservableProperty] private string _currentMonthLabel = "";

    // ── Chart data (last 4 calendar weeks) ───────────────────────────────────
    [ObservableProperty] private double[] _weeklyVolumes = [0, 0, 0, 0];
    [ObservableProperty] private string[] _weekLabels = ["W1", "W2", "W3", "W4"];

    // ── Filter display ────────────────────────────────────────────────────────
    public string FilterLabel => SelectedPeriod == "All time" ? "FILTER" : SelectedPeriod.ToUpper();

    public HistoryViewModel(IWorkoutHistoryService historyService)
    {
        _historyService = historyService;
        _currentMonthLabel = DateTime.Now.ToString("MMMM yyyy").ToUpper();
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            // Always load all time for chart + stats
            _allWorkouts = [.. await _historyService.GetHistoryAsync()];
            ComputeMonthlyStats();
            ComputeWeeklyChart();
            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ApplyFilter()
    {
        var (from, to) = ResolveDateRange(SelectedPeriod);
        var filtered = _allWorkouts.AsEnumerable();
        if (from.HasValue) filtered = filtered.Where(w => w.CompletedAt >= from.Value);
        if (to.HasValue)   filtered = filtered.Where(w => w.CompletedAt <= to.Value);
        if (SelectedTag is not null) filtered = filtered.Where(w => w.TagNames.Contains(SelectedTag.Name));

        Workouts = [.. filtered];
        IsEmpty = Workouts.Count == 0;
        OnPropertyChanged(nameof(FilterLabel));
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedPeriod = "All time";
        SelectedTag = null;
        ApplyFilter();
    }

    [RelayCommand]
    private async Task OpenWorkoutAsync(WorkoutHistorySummaryDto workout)
    {
        await Shell.Current.GoToAsync($"WorkoutDetail?workoutId={workout.Id}");
    }

    [RelayCommand]
    private async Task DeleteWorkoutAsync(WorkoutHistorySummaryDto workout)
    {
        bool confirmed = await Shell.Current.DisplayAlert(
            "Delete Workout",
            $"Permanently delete \"{workout.DisplayName}\"?",
            "Delete", "Cancel");
        if (!confirmed) return;

        await _historyService.DeleteWorkoutAsync(workout.Id);
        WeakReferenceMessenger.Default.Send(new WorkoutChangedMessage(workout.Id));
        _allWorkouts = _allWorkouts.Where(w => w.Id != workout.Id).ToList();
        ComputeMonthlyStats();
        ComputeWeeklyChart();
        ApplyFilter();
    }

    private void ComputeMonthlyStats()
    {
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Local);
        var thisMonth = _allWorkouts.Where(w => w.CompletedAt.ToLocalTime() >= monthStart).ToList();
        MonthSessionCount = thisMonth.Count.ToString();
        var vol = thisMonth.Sum(w => w.TotalVolumeKg);
        MonthVolumeText = $"{vol:0} kg";
        CurrentMonthLabel = now.ToString("MMMM yyyy").ToUpper();
    }

    private void ComputeWeeklyChart()
    {
        // Anchor to start of current week (Monday)
        var today = DateTime.Now.Date;
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        var thisWeekStart = today.AddDays(-daysSinceMonday);

        var volumes = new double[4];
        var labels = new string[4];
        for (int i = 0; i < 4; i++)
        {
            var weekStart = thisWeekStart.AddDays(-7 * (3 - i));
            var weekEnd = weekStart.AddDays(7);
            labels[i] = $"W{i + 1}";
            volumes[i] = _allWorkouts
                .Where(w =>
                {
                    var local = w.CompletedAt.ToLocalTime().Date;
                    return local >= weekStart && local < weekEnd;
                })
                .Sum(w => w.TotalVolumeKg);
        }

        WeeklyVolumes = volumes;
        WeekLabels = labels;
    }

    private static (DateTime? from, DateTime? to) ResolveDateRange(string period) => period switch
    {
        "This week" => (DateTime.UtcNow.Date.AddDays(-(((int)DateTime.UtcNow.DayOfWeek + 6) % 7)), null),
        "This month" => (new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc), null),
        "Last 3 months" => (DateTime.UtcNow.AddMonths(-3), null),
        _ => (null, null)
    };
}
