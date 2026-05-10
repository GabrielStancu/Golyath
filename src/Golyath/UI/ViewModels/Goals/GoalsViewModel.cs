using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.UI.Views.Goals;

namespace Golyath.UI.ViewModels.Goals;

public partial class GoalsViewModel : ObservableObject
{
    private readonly IGoalService _goalService;
    private readonly IUserService _userService;
    private readonly IPersonalRecordService _prService;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private ObservableCollection<GoalSummary> _activeGoals = [];
    [ObservableProperty] private ObservableCollection<GoalSummary> _completedGoals = [];
    [ObservableProperty] private bool _hasActiveGoals;
    [ObservableProperty] private bool _hasCompletedGoals;
    [ObservableProperty] private bool _isEmpty;

    // ── Tab state ────────────────────────────────────────────────────────────
    [ObservableProperty] private bool _showGoals = true;
    [ObservableProperty] private bool _showRecords;

    // ── Personal Records ─────────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<PersonalRecord> _personalRecords = [];
    [ObservableProperty] private bool _hasPersonalRecords;
    [ObservableProperty] private bool _isRecordsEmpty;

    public GoalsViewModel(IGoalService goalService, IUserService userService, IPersonalRecordService prService)
    {
        _goalService = goalService;
        _userService = userService;
        _prService = prService;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user is null) return;

            var goals = await _goalService.GetGoalsAsync(user.Id);

            ActiveGoals = new ObservableCollection<GoalSummary>(
                goals.Where(g => !g.IsCompleted));
            CompletedGoals = new ObservableCollection<GoalSummary>(
                goals.Where(g => g.IsCompleted));

            HasActiveGoals = ActiveGoals.Count > 0;
            HasCompletedGoals = CompletedGoals.Count > 0;
            IsEmpty = goals.Count == 0;

            var records = await _prService.GetPersonalRecordsAsync(user.Id);
            PersonalRecords = new ObservableCollection<PersonalRecord>(records);
            HasPersonalRecords = PersonalRecords.Count > 0;
            IsRecordsEmpty = PersonalRecords.Count == 0;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SwitchToGoals()
    {
        ShowGoals = true;
        ShowRecords = false;
    }

    [RelayCommand]
    private void SwitchToRecords()
    {
        ShowGoals = false;
        ShowRecords = true;
    }

    [RelayCommand]
    private async Task NavigateToAddGoalAsync()
    {
        await Shell.Current.GoToAsync(nameof(AddGoalPage));
    }

    [RelayCommand]
    private async Task DeleteGoalAsync(GoalSummary goal)
    {
        bool confirmed = await Shell.Current.DisplayAlert(
            "Delete Goal",
            $"Delete \"{goal.Description}\"?",
            "Delete",
            "Cancel");

        if (!confirmed) return;

        await _goalService.DeleteGoalAsync(goal.Id);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task CompleteGoalAsync(GoalSummary goal)
    {
        await _goalService.CompleteGoalAsync(goal.Id);
        await LoadAsync();
    }
}
