using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.UI.ViewModels.Workout;
using Golyath.UI.Views.Workout;

namespace Golyath.UI.ViewModels.Dashboard;

public partial class DashboardViewModel : ObservableObject, IRecipient<WorkoutChangedMessage>
{
    private readonly IDashboardService _dashboardService;
    private readonly IUserService _userService;

    // ── Header ──────────────────────────────────────────────────────────────
    [ObservableProperty] private string _greetingLine = string.Empty;
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private string _userInitial = string.Empty;
    [ObservableProperty] private bool _isLoading = true;

    // ── Stats pills ──────────────────────────────────────────────────────────
    [ObservableProperty] private string _weeklyWorkoutCount = "0";
    [ObservableProperty] private string _weeklyVolume = "0";
    [ObservableProperty] private string _streakCount = "0";

    // ── Hero card ────────────────────────────────────────────────────────────
    [ObservableProperty] private string _heroTitle = "Free Workout";
    [ObservableProperty] private string _heroSubtitle = "Add exercises on the fly";

    public DashboardViewModel(IDashboardService dashboardService, IUserService userService)
    {
        _dashboardService = dashboardService;
        _userService = userService;
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(WorkoutChangedMessage message) => _ = LoadAsync();

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var user = await _userService.GetCurrentUserAsync();
            var hour = DateTime.Now.Hour;
            GreetingLine = hour < 12 ? "GOOD MORNING" : hour < 17 ? "GOOD AFTERNOON" : "GOOD EVENING";
            UserName = user?.Nickname is { Length: > 0 } nick ? nick.ToUpperInvariant() : "ATHLETE";
            UserInitial = UserName.Length > 0 ? UserName[..1] : "A";

            var countTask = _dashboardService.GetWeeklyWorkoutCountAsync();
            var volumeTask = _dashboardService.GetWeeklyVolumeAsync();
            var streakTask = _dashboardService.GetWeekStreakAsync();
            var suggestionTask = _dashboardService.GetWorkoutSuggestionAsync();

            await Task.WhenAll(countTask, volumeTask, streakTask, suggestionTask);

            WeeklyWorkoutCount = countTask.Result.ToString();
            WeeklyVolume = (volumeTask.Result / 1000.0).ToString("F1");
            StreakCount = streakTask.Result.ToString();

            HeroTitle = "Free Workout";
            HeroSubtitle = "Add exercises on the fly";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartWorkout()
    {
        await Shell.Current.GoToAsync(nameof(ActiveWorkoutPage));
    }

    [RelayCommand]
    private async Task OpenProfile()
    {
        await Shell.Current.GoToAsync(nameof(Golyath.UI.Views.Profile.EditProfilePage));
    }
}
