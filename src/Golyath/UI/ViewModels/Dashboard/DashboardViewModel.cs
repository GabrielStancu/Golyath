using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.UI.Views.Workout;

namespace Golyath.UI.ViewModels.Dashboard;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;
    private readonly IUserService _userService;

    // ── Header ──────────────────────────────────────────────────────────────
    [ObservableProperty] private string _greeting = string.Empty;
    [ObservableProperty] private string _todayLabel = string.Empty;
    [ObservableProperty] private bool _isLoading = true;

    // ── Last workout ─────────────────────────────────────────────────────────
    [ObservableProperty] private bool _hasLastWorkout;
    [ObservableProperty] private string _lastWorkoutTitle = "No workouts yet";
    [ObservableProperty] private string _lastWorkoutDate = string.Empty;
    [ObservableProperty] private string _lastWorkoutSets = string.Empty;
    [ObservableProperty] private string _lastWorkoutVolume = string.Empty;
    [ObservableProperty] private string _lastWorkoutDuration = string.Empty;

    // ── Weekly activity ───────────────────────────────────────────────────────
    [ObservableProperty] private IReadOnlyList<WeeklyActivityDay> _weeklyDays = [];

    // ── Readiness ────────────────────────────────────────────────────────────
    [ObservableProperty] private string _readinessEmoji = "🔥";
    [ObservableProperty] private string _readinessLabel = string.Empty;
    [ObservableProperty] private string _readinessMessage = string.Empty;
    [ObservableProperty] private double _readinessValue;
    [ObservableProperty] private Color _readinessGaugeColor = Color.FromArgb("#2E7D32");

    // ── Suggestion ───────────────────────────────────────────────────────────
    [ObservableProperty] private string _suggestionMuscle = string.Empty;
    [ObservableProperty] private string _suggestionReason = string.Empty;

    public bool HasNoLastWorkout => !HasLastWorkout;

    public DashboardViewModel(IDashboardService dashboardService, IUserService userService)
    {
        _dashboardService = dashboardService;
        _userService = userService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            TodayLabel = DateTime.Now.ToString("dddd, MMMM d");

            var user = await _userService.GetCurrentUserAsync();
            var hour = DateTime.Now.Hour;
            var timeOfDay = hour < 12 ? "Good morning" : hour < 17 ? "Good afternoon" : "Good evening";
            Greeting = user?.Nickname is { Length: > 0 } nick
                ? $"{timeOfDay}, {nick}!"
                : $"{timeOfDay}!";

            var lastTask = _dashboardService.GetLastWorkoutSummaryAsync();
            var weeklyTask = _dashboardService.GetWeeklyActivityAsync();
            var readinessTask = _dashboardService.GetReadinessAsync();
            var suggestionTask = _dashboardService.GetWorkoutSuggestionAsync();

            await Task.WhenAll(lastTask, weeklyTask, readinessTask, suggestionTask);

            ApplyLastWorkout(lastTask.Result);
            BuildWeeklyChart(weeklyTask.Result);
            ApplyReadiness(readinessTask.Result);
            ApplySuggestion(suggestionTask.Result);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyLastWorkout(LastWorkoutSummary? summary)
    {
        HasLastWorkout = summary is not null;
        OnPropertyChanged(nameof(HasNoLastWorkout));

        if (summary is null) return;

        LastWorkoutTitle = string.IsNullOrWhiteSpace(summary.Name) ? "Workout" : summary.Name;

        var local = summary.CompletedAt.ToLocalTime();
        var daysAgo = (DateTime.Today - local.Date).Days;
        LastWorkoutDate = daysAgo switch
        {
            0 => "Today",
            1 => "Yesterday",
            _ => $"{daysAgo} days ago"
        };

        LastWorkoutSets = $"{summary.SetCount} sets";
        LastWorkoutVolume = $"{summary.TotalVolumeKg:F0} kg";
        LastWorkoutDuration = FormatDuration(summary.DurationSeconds);
    }

    private void BuildWeeklyChart(IReadOnlyList<WeeklyActivityDay> days)
    {
        WeeklyDays = days;
    }

    private void ApplyReadiness(ReadinessInfo info)
    {
        (ReadinessGaugeColor, ReadinessEmoji, ReadinessValue) = info.Level switch
        {
            ReadinessLevel.Rest     => (Color.FromArgb("#E53935"), "💤", 0.15),
            ReadinessLevel.Moderate => (Color.FromArgb("#F9A825"), "⚡", 0.55),
            ReadinessLevel.Ready    => (Color.FromArgb("#2E7D32"), "🔥", 1.0),
            _                       => (Color.FromArgb("#2E7D32"), "🔥", 1.0)
        };
        ReadinessLabel = info.Label;
        ReadinessMessage = info.Message;
    }

    private void ApplySuggestion(WorkoutSuggestion suggestion)
    {
        SuggestionMuscle = suggestion.MuscleGroupName;
        SuggestionReason = suggestion.Reason;
    }

    [RelayCommand]
    private async Task StartWorkout()
    {
        await Shell.Current.GoToAsync(nameof(ActiveWorkoutPage));
    }

    private static string FormatDuration(int seconds)
    {
        if (seconds <= 0) return "—";
        var ts = TimeSpan.FromSeconds(seconds);
        return ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours}h {ts.Minutes}m"
            : $"{ts.Minutes}m";
    }
}
