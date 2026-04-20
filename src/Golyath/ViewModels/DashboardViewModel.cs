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

    private static readonly string[] WeekDays = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    // -- Stats --
    [ObservableProperty] private int _workoutsThisWeek;
    [ObservableProperty] private string _totalVolumeThisWeek = "0 kg";
    [ObservableProperty] private string _avgDuration = "0 min";
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

    // -- Recent workouts (fake for Phase 1) --
    [ObservableProperty] private ObservableCollection<RecentWorkoutItem> _recentWorkouts = [];

    public DashboardViewModel(IWorkoutService workoutService)
    {
        _workoutService = workoutService;
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

            var last = await _workoutService.GetLastSessionAsync();
            HasLastWorkout = last is not null;
            if (last is not null)
                LastWorkoutSummary = last.StartedAt.ToLocalTime().ToString("ddd, MMM d");

            // -- Fake data until real analytics is built (Phase 3) --
            BuildFakeData(vol);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void BuildFakeData(double realVol)
    {
        // Fake weekly volume per day (kg), Sunday = index 6
        float[] fakeVolumes = [4200f, 0f, 5800f, 3400f, 6100f, 0f, 2100f];

        // Today index: Monday=0 .. Sunday=6
        int todayIdx = ((int)DateTime.Today.DayOfWeek + 6) % 7;

        // Override today's bar with a real-ish value if we have real data
        if (realVol > 0)
            fakeVolumes[todayIdx] = (float)realVol;

        VolumeChart = new WeeklyVolumeChartDrawable
        {
            Values = fakeVolumes,
            Labels = WeekDays,
            HighlightIndex = todayIdx
        };

        // Fake weekly stats
        int fakeSessions = WorkoutsThisWeek > 0 ? WorkoutsThisWeek : 4;
        double fakeVolKg = realVol > 0 ? realVol : fakeVolumes.Sum();
        TotalVolumeThisWeek = $"{fakeVolKg:N0} kg";
        AvgDuration = "52 min";

        // Fake recent workouts
        var today = DateTime.Today;
        RecentWorkouts =
        [
            new("Today", today.ToString("MMM d"), "58 min", "6 200 kg", "6 exercises"),
            new("Thu", today.AddDays(-2).ToString("MMM d"), "44 min", "3 400 kg", "5 exercises"),
            new("Tue", today.AddDays(-4).ToString("MMM d"), "61 min", "5 800 kg", "7 exercises"),
        ];
    }

    [RelayCommand]
    private async Task GoToSettingsAsync() =>
        await Shell.Current.GoToAsync("settings");
}
