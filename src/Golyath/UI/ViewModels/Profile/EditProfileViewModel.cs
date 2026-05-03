using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.Services;
using Golyath.Core.Enums;
using Golyath.Core.Utilities;

namespace Golyath.UI.ViewModels.Profile;

public partial class EditProfileViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IThemeService _themeService;

    public EditProfileViewModel(IUserService userService, IThemeService themeService)
    {
        _userService = userService;
        _themeService = themeService;
    }

    public event EventHandler? SaveCompleted;

    [ObservableProperty] private string _nickname = string.Empty;
    [ObservableProperty] private DateTime _birthday = DateTime.Today.AddYears(-25);

    public DateTime MaxBirthday { get; } = DateTime.Today.AddYears(-10);

    [ObservableProperty] private string _heightInput = string.Empty;
    [ObservableProperty] private string _weightInput = string.Empty;
    [ObservableProperty] private string _heightLabel = "Height (cm)";
    [ObservableProperty] private string _weightLabel = "Weight (kg)";
    [ObservableProperty] private bool _isImperialUnit;

    public bool IsMetricSelected => !IsImperialUnit;

    [ObservableProperty] private string _selectedGenderDisplay = "Prefer not to say";
    [ObservableProperty] private string _selectedGoalDisplay = "Balanced";
    [ObservableProperty] private bool _isDarkMode;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSaveMessage))]
    private string? _saveMessage;

    public bool HasSaveMessage => !string.IsNullOrEmpty(SaveMessage);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    public IReadOnlyList<string> GenderOptions { get; } =
        ["Male", "Female", "Other", "Prefer not to say"];

    public IReadOnlyList<string> GoalOptions { get; } =
        ["Strength", "Hypertrophy", "Fat Loss", "Balanced"];

    partial void OnIsImperialUnitChanged(bool value)
    {
        OnPropertyChanged(nameof(IsMetricSelected));
        var ci = System.Globalization.CultureInfo.InvariantCulture;

        if (value)
        {
            if (double.TryParse(HeightInput, System.Globalization.NumberStyles.Any, ci, out var h))
                HeightInput = Math.Round(UnitConversion.CmToInches(h), 1).ToString("F1", ci);
            if (double.TryParse(WeightInput, System.Globalization.NumberStyles.Any, ci, out var w))
                WeightInput = Math.Round(UnitConversion.KgToLb(w), 1).ToString("F1", ci);
            HeightLabel = "Height (in)";
            WeightLabel = "Weight (lb)";
        }
        else
        {
            if (double.TryParse(HeightInput, System.Globalization.NumberStyles.Any, ci, out var h))
                HeightInput = Math.Round(UnitConversion.InchesToCm(h), 1).ToString("F1", ci);
            if (double.TryParse(WeightInput, System.Globalization.NumberStyles.Any, ci, out var w))
                WeightInput = Math.Round(UnitConversion.LbToKg(w), 1).ToString("F1", ci);
            HeightLabel = "Height (cm)";
            WeightLabel = "Weight (kg)";
        }
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        _themeService.ApplyTheme(value ? AppTheme.Dark : AppTheme.Light);
    }

    [RelayCommand]
    private void SelectMetric() { if (IsImperialUnit) IsImperialUnit = false; }

    [RelayCommand]
    private void SelectImperial() { if (!IsImperialUnit) IsImperialUnit = true; }

    public async Task InitializeAsync()
    {
        var user = await _userService.GetCurrentUserAsync();
        if (user is null) return;

        Nickname = user.Nickname;
        Birthday = user.Birthday;
        IsImperialUnit = user.PreferredUnit == WeightUnit.Lb;
        IsDarkMode = _themeService.GetPreferredTheme() == AppTheme.Dark;

        var ci = System.Globalization.CultureInfo.InvariantCulture;

        // Set display values (conversion already handled by OnIsImperialUnitChanged side-effect)
        // Set raw metric values first; the property change will convert if needed
        HeightInput = user.HeightCm.ToString("F1", ci);
        WeightInput = user.WeightKg.ToString("F2", ci);

        SelectedGenderDisplay = user.Gender switch
        {
            Gender.Male   => "Male",
            Gender.Female => "Female",
            Gender.Other  => "Other",
            _             => "Prefer not to say"
        };

        SelectedGoalDisplay = user.FitnessGoal switch
        {
            FitnessGoal.Strength    => "Strength",
            FitnessGoal.Hypertrophy => "Hypertrophy",
            FitnessGoal.FatLoss     => "Fat Loss",
            _                       => "Balanced"
        };
    }

    [RelayCommand]
    private async Task Save()
    {
        IsBusy = true;
        SaveMessage = null;

        try
        {
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            if (!double.TryParse(HeightInput, System.Globalization.NumberStyles.Any, ci, out var height) || height <= 0 ||
                !double.TryParse(WeightInput, System.Globalization.NumberStyles.Any, ci, out var weight) || weight <= 0)
            {
                SaveMessage = "Please enter valid height and weight.";
                return;
            }

            var heightCm = IsImperialUnit ? UnitConversion.InchesToCm(height) : height;
            var weightKg = IsImperialUnit ? UnitConversion.LbToKg(weight) : weight;

            var gender = SelectedGenderDisplay switch
            {
                "Male"   => Gender.Male,
                "Female" => Gender.Female,
                "Other"  => Gender.Other,
                _        => Gender.PreferNotToSay
            };

            var goal = SelectedGoalDisplay switch
            {
                "Strength"    => FitnessGoal.Strength,
                "Hypertrophy" => FitnessGoal.Hypertrophy,
                "Fat Loss"    => FitnessGoal.FatLoss,
                _             => FitnessGoal.Balanced
            };

            var user = await _userService.GetCurrentUserAsync();
            if (user is null) return;

            user.Nickname    = Nickname.Trim();
            user.Birthday    = Birthday;
            user.HeightCm    = Math.Round(heightCm, 1);
            user.WeightKg    = Math.Round(weightKg, 2);
            user.Gender      = gender;
            user.FitnessGoal = goal;
            user.PreferredUnit = IsImperialUnit ? WeightUnit.Lb : WeightUnit.Kg;

            await _userService.UpdateUserAsync(user);
            SaveMessage = "Profile saved!";
            SaveCompleted?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
