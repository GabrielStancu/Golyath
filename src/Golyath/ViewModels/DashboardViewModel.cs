using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Charts;
using Golyath.Services;
using System.Collections.ObjectModel;

namespace Golyath.ViewModels;

public record RecentWorkoutItem(string DayLabel, string DateLabel, string DurationLabel, string VolumeLabel, string ExerciseCount);

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IWorkoutService _workoutService;
    private readonly IAnalyticsService _analyticsService;

    private static readonly string[] WeekDays = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    // -- Stats --
    [ObservableProperty] private int _workoutsThisWeek;
    [ObservableProperty] private string _totalVolumeThisWeek = "0 kg";
    [ObservableProperty] private string _avgDuration = "— min";
    [ObservableProperty] private string _greeting = "Good morning";
    [ObservableProperty] private string _dateLabel = string.Empty;

    // -- Last workout --
    [ObservableProperty] private string _lastWorkoutSummary = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoLastWorkout))]
    private bool _hasLastWorkout;
    public bool HasNoLastWorkout => !HasLastWorkout;

    // -- Chart --
    [ObservableProperty] private WeeklyVolumeChartDrawable _volumeChart = new();

    // -- Recent workouts --
    [ObservableProperty] private ObservableCollection<RecentWorkoutItem> _recentWorkouts = [];

    public DashboardViewModel(IWorkoutService workoutService, IAnalyticsService analyticsService)
    {
        _workoutService = workoutService;
        _analyticsService = analyticsService;
        Title = "Golyath";
        SetGreeting();
    }

    private void SetGreeting()
    {
        int hour = DateTime.Now.Hour;
        Greeting = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
        DateLabel = DateTime.Today.ToString("dddd, MMM d");
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            WorkoutsThisWeek = await _workoutService.GetWorkoutsThisWeekAsync();
            double vol = await _workoutService.GetTotalVolumeThisWeekAsync();
            TotalVolumeThisWeek = vol > 0 ? $"{vol:N0} kg" : "0 kg";

            double avgMin = await _workoutService.GetAvgDurationMinutesAsync(30);
            AvgDuration = avgMin > 0 ? $"{(int)avgMin} min" : "— min";

            var last = await _workoutService.GetLastSessionAsync();
            HasLastWorkout = last is not null;
            if (last is not null)
                LastWorkoutSummary = last.StartedAt.ToLocalTime().ToString("ddd, MMM d");

            // Real weekly volume chart
            float[] weeklyVolumes = await _analyticsService.GetCurrentWeekVolumeAsync();
            int todayIdx = ((int)DateTime.Today.DayOfWeek + 6) % 7;
            VolumeChart = new WeeklyVolumeChartDrawable
            {
                Values = weeklyVolumes,
                Labels = WeekDays,
                HighlightIndex = todayIdx
            };

            // Real recent workouts (last 5)
            await BuildRecentWorkoutsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task BuildRecentWorkoutsAsync()
    {
        var sessions = await _workoutService.GetRecentSessionsAsync(5);
        var items = new List<RecentWorkoutItem>();
        var today = DateTime.Today;

        foreach (var session in sessions)
        {
            var sets = await _workoutService.GetSetsForSessionAsync(session.Id);
            var exerciseNames = await _workoutService.GetExerciseNamesForSessionAsync(session.Id);

            double sessionVol = sets.Sum(s => s.Volume);
            int durationMin = (int)session.Duration.TotalMinutes;
            int exerciseCount = exerciseNames.Count;

            var localDate = session.StartedAt.ToLocalTime().Date;
            string dayLabel = localDate == today ? "Today"
                : localDate == today.AddDays(-1) ? "Yest."
                : localDate.ToString("ddd");
            string dateLabel = localDate.ToString("MMM d");

            items.Add(new RecentWorkoutItem(
                dayLabel,
                dateLabel,
                durationMin > 0 ? $"{durationMin} min" : "—",
                sessionVol > 0 ? $"{sessionVol:N0} kg" : "0 kg",
                exerciseCount == 1 ? "1 exercise" : $"{exerciseCount} exercises"));
        }

        RecentWorkouts = new ObservableCollection<RecentWorkoutItem>(items);
    }

    [RelayCommand]
    private async Task GoToSettingsAsync() =>
        await Shell.Current.GoToAsync("settings");
}

