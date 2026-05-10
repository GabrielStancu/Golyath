using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.Core.Enums;

namespace Golyath.UI.ViewModels.Goals;

public partial class AddGoalViewModel : ObservableObject
{
    private readonly IGoalService _goalService;
    private readonly IUserService _userService;

    // ── Goal type selection ──────────────────────────────────────────────────
    public static readonly IReadOnlyList<string> GoalTypeLabels = ["Strength", "Frequency", "Balance"];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowExercisePicker))]
    [NotifyPropertyChangedFor(nameof(TargetLabel))]
    [NotifyPropertyChangedFor(nameof(TargetHint))]
    private string _selectedGoalTypeLabel = "Strength";

    // ── Form fields ──────────────────────────────────────────────────────────
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _targetValueText = string.Empty;
    [ObservableProperty] private ExerciseOption? _selectedExercise;
    [ObservableProperty] private ObservableCollection<ExerciseOption> _exercises = [];
    [ObservableProperty] private bool _hasTargetDate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TargetDateText))]
    private DateTime _targetDate = DateTime.Today.AddMonths(3);

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // ── Computed properties ──────────────────────────────────────────────────
    public bool ShowExercisePicker => SelectedGoalType == GoalType.Strength;

    public string TargetLabel => SelectedGoalType switch
    {
        GoalType.Strength => "TARGET WEIGHT (KG)",
        GoalType.Frequency => "TARGET WORKOUTS PER WEEK",
        GoalType.Balance => "TARGET MUSCLE GROUPS PER WEEK",
        _ => "TARGET"
    };

    public string TargetHint => SelectedGoalType switch
    {
        GoalType.Strength => "e.g. 100",
        GoalType.Frequency => "e.g. 4",
        GoalType.Balance => "e.g. 5",
        _ => string.Empty
    };

    public string TargetDateText => TargetDate.ToString("MMM d, yyyy");

    private GoalType SelectedGoalType => SelectedGoalTypeLabel switch
    {
        "Frequency" => GoalType.Frequency,
        "Balance" => GoalType.Balance,
        _ => GoalType.Strength
    };

    public AddGoalViewModel(
        IGoalService goalService,
        IUserService userService)
    {
        _goalService = goalService;
        _userService = userService;
    }

    public async Task LoadAsync()
    {
        var allExercises = await _goalService.GetAllExercisesAsync();
        Exercises = new ObservableCollection<ExerciseOption>(allExercises);
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Description))
        {
            ErrorMessage = "Please enter a description for your goal.";
            return;
        }

        if (!double.TryParse(TargetValueText, out double targetValue) || targetValue <= 0)
        {
            ErrorMessage = "Please enter a valid target value greater than 0.";
            return;
        }

        if (SelectedGoalType == GoalType.Strength && SelectedExercise is null)
        {
            ErrorMessage = "Please select an exercise for the strength goal.";
            return;
        }

        var user = await _userService.GetCurrentUserAsync();
        if (user is null) return;

        IsBusy = true;
        try
        {
            await _goalService.CreateGoalAsync(
                userId: user.Id,
                type: SelectedGoalType,
                description: Description,
                targetValue: targetValue,
                exerciseId: SelectedGoalType == GoalType.Strength ? SelectedExercise?.Id : null,
                targetDate: HasTargetDate ? TargetDate.ToUniversalTime() : null);

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void SelectGoalType(string label)
    {
        SelectedGoalTypeLabel = label;
    }
}
