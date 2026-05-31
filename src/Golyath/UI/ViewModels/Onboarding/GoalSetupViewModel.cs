using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.Services;
using Golyath.Core.Enums;

namespace Golyath.UI.ViewModels.Onboarding;

public record GoalItem(FitnessGoal Goal, string Title, string Description, string Icon);

public partial class GoalSetupViewModel : ObservableObject
{
    private readonly OnboardingDataService _onboardingData;
    private readonly IUserService _userService;

    public GoalSetupViewModel(OnboardingDataService onboardingData, IUserService userService)
    {
        _onboardingData = onboardingData;
        _userService = userService;
    }

    public event EventHandler? CompletedRequested;

    public IReadOnlyList<GoalItem> Goals { get; } =
    [
        new(FitnessGoal.Strength,    "Strength",    "Build raw power and lift heavier",         "\uEA4A"),
        new(FitnessGoal.Hypertrophy, "Hypertrophy", "Maximize muscle size and definition",      "\uEA4A"),
        new(FitnessGoal.FatLoss,     "Fat Loss",    "Burn fat while preserving muscle",          "\uE80E"),
        new(FitnessGoal.Balanced,    "Balanced",    "All-around fitness and health",             "\uE8D5"),
    ];

    [ObservableProperty]
    private FitnessGoal _selectedGoal = FitnessGoal.Balanced;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    // Computed selection flags used by XAML DataTriggers
    public bool IsStrengthSelected    => SelectedGoal == FitnessGoal.Strength;
    public bool IsHypertrophySelected => SelectedGoal == FitnessGoal.Hypertrophy;
    public bool IsFatLossSelected     => SelectedGoal == FitnessGoal.FatLoss;
    public bool IsBalancedSelected    => SelectedGoal == FitnessGoal.Balanced;

    partial void OnSelectedGoalChanged(FitnessGoal value)
    {
        OnPropertyChanged(nameof(IsStrengthSelected));
        OnPropertyChanged(nameof(IsHypertrophySelected));
        OnPropertyChanged(nameof(IsFatLossSelected));
        OnPropertyChanged(nameof(IsBalancedSelected));
    }

    [RelayCommand]
    private void SelectGoal(FitnessGoal goal) => SelectedGoal = goal;

    [RelayCommand]
    private async Task Complete()
    {
        IsBusy = true;
        try
        {
            _onboardingData.FitnessGoal = SelectedGoal;
            await _userService.CreateUserAsync(
                _onboardingData.Nickname,
                _onboardingData.Birthday,
                _onboardingData.HeightCm,
                _onboardingData.WeightKg,
                _onboardingData.Gender,
                _onboardingData.FitnessGoal,
                _onboardingData.PreferredUnit,
                _onboardingData.Language);

            CompletedRequested?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
