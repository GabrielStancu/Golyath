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

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private ObservableCollection<GoalSummary> _activeGoals = [];
    [ObservableProperty] private ObservableCollection<GoalSummary> _completedGoals = [];
    [ObservableProperty] private bool _hasActiveGoals;
    [ObservableProperty] private bool _hasCompletedGoals;
    [ObservableProperty] private bool _isEmpty;

    public GoalsViewModel(IGoalService goalService, IUserService userService)
    {
        _goalService = goalService;
        _userService = userService;
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
        }
        finally
        {
            IsLoading = false;
        }
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
