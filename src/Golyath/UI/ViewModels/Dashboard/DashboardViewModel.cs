using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.UI.ViewModels.Workout;
using Golyath.UI.Views.Workout;

namespace Golyath.UI.ViewModels.Dashboard;

public partial class DashboardViewModel : ObservableObject, IRecipient<WorkoutChangedMessage>, IRecipient<RoutineChangedMessage>
{
    private readonly IDashboardService _dashboardService;
    private readonly IUserService _userService;
    private readonly IRoutineService _routineService;
    private readonly IWorkoutService _workoutService;

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
    private int? _heroRoutineId;

    // ── Routines ─────────────────────────────────────────────────────────────
    public ObservableCollection<RoutineSummaryDto> Routines { get; } = [];
    [ObservableProperty] private bool _hasRoutines;
    [ObservableProperty] private bool _hasNoRoutines = true;

    public DashboardViewModel(
        IDashboardService dashboardService,
        IUserService userService,
        IRoutineService routineService,
        IWorkoutService workoutService)
    {
        _dashboardService = dashboardService;
        _userService = userService;
        _routineService = routineService;
        _workoutService = workoutService;
        WeakReferenceMessenger.Default.Register<WorkoutChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<RoutineChangedMessage>(this);
    }

    public void Receive(WorkoutChangedMessage message) => _ = LoadAsync();
    public void Receive(RoutineChangedMessage message) => _ = LoadAsync();

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
            var nextRoutineTask = _routineService.GetNextRoutineInRotationAsync();
            var routinesTask = _routineService.GetTopRoutinesAsync(3);

            await Task.WhenAll(countTask, volumeTask, streakTask, suggestionTask, nextRoutineTask, routinesTask);

            WeeklyWorkoutCount = countTask.Result.ToString();
            WeeklyVolume = (volumeTask.Result / 1000.0).ToString("F1");
            StreakCount = streakTask.Result.ToString();

            // Hero: show next routine in rotation, fallback to free workout
            var nextRoutine = nextRoutineTask.Result;
            if (nextRoutine is not null)
            {
                HeroTitle = nextRoutine.Name;
                HeroSubtitle = $"{nextRoutine.ExerciseLabel}  ·  {nextRoutine.DurationLabel}";
                _heroRoutineId = nextRoutine.Id;
            }
            else
            {
                HeroTitle = "Free Workout";
                HeroSubtitle = "Add exercises on the fly";
                _heroRoutineId = null;
            }

            // Routines list (max 3)
            Routines.Clear();
            foreach (var r in routinesTask.Result)
                Routines.Add(r);
            HasRoutines = Routines.Count > 0;
            HasNoRoutines = !HasRoutines;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartWorkout()
    {
        if (_heroRoutineId is { } id)
        {
            var workout = await _workoutService.StartWorkoutFromRoutineAsync(id);
            await Shell.Current.GoToAsync($"{nameof(ActiveWorkoutPage)}?workoutId={workout.Id}");
        }
        else
        {
            await Shell.Current.GoToAsync(nameof(ActiveWorkoutPage));
        }
    }

    [RelayCommand]
    private async Task StartFreeWorkout()
    {
        await Shell.Current.GoToAsync(nameof(ActiveWorkoutPage));
    }

    [RelayCommand]
    private async Task OpenProfile()
    {
        await Shell.Current.GoToAsync(nameof(Golyath.UI.Views.Profile.EditProfilePage));
    }

    [RelayCommand]
    private async Task StartRoutine(RoutineSummaryDto routine)
    {
        var workout = await _workoutService.StartWorkoutFromRoutineAsync(routine.Id);
        await Shell.Current.GoToAsync($"{nameof(ActiveWorkoutPage)}?workoutId={workout.Id}");
    }

    [RelayCommand]
    private async Task EditRoutine(RoutineSummaryDto routine)
    {
        await Shell.Current.GoToAsync($"{nameof(RoutineBuilderPage)}?routineId={routine.Id}");
    }

    [RelayCommand]
    private async Task NewRoutine()
    {
        await Shell.Current.GoToAsync(nameof(RoutineBuilderPage));
    }

    [RelayCommand]
    private async Task SeeAllRoutines()
    {
        // Switch to TRAIN tab
        if (Shell.Current is Shell shell)
            shell.CurrentItem = shell.Items.FirstOrDefault(i =>
                i.Route?.Contains("WorkoutTemplates", StringComparison.OrdinalIgnoreCase) == true) ?? shell.CurrentItem;
    }
}
