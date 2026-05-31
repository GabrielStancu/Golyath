using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.Localization;
using Golyath.Application.Services;
using Golyath.Core.Enums;
using Golyath.Core.Utilities;

namespace Golyath.UI.ViewModels.Onboarding;

public partial class ProfileSetupViewModel : ObservableObject
{
    private readonly OnboardingDataService _onboardingData;

    public ProfileSetupViewModel(OnboardingDataService onboardingData)
    {
        _onboardingData = onboardingData;
    }

    public event EventHandler? ContinueRequested;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand))]
    private string _nickname = string.Empty;

    [ObservableProperty]
    private DateTime _birthday = DateTime.Today.AddYears(-25);

    public DateTime MaxBirthday { get; } = DateTime.Today.AddYears(-10);

    [ObservableProperty]
    private string _heightInput = "170";

    [ObservableProperty]
    private string _weightInput = "70";

    [ObservableProperty]
    private string _heightLabel = LocalizationManager.Instance["Profile_Height_Cm"];

    [ObservableProperty]
    private string _weightLabel = LocalizationManager.Instance["Profile_Weight_Kg"];

    [ObservableProperty]
    private bool _isImperialUnit;

    [ObservableProperty]
    private string? _validationMessage;

    public bool HasValidationMessage => !string.IsNullOrEmpty(ValidationMessage);

    partial void OnValidationMessageChanged(string? value) =>
        OnPropertyChanged(nameof(HasValidationMessage));

    public IReadOnlyList<string> GenderOptions { get; } =
        ["Male", "Female", "Prefer not to say"];

    [ObservableProperty]
    private string _selectedGenderDisplay = "Prefer not to say";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLanguageFlag))]
    [NotifyPropertyChangedFor(nameof(SelectedLanguageName))]
    private AppLanguage _selectedLanguage = AppLanguage.English;

    public string SelectedLanguageFlag => UI.Controls.LanguageSelectionPopup.FlagFor(SelectedLanguage);
    public string SelectedLanguageName => UI.Controls.LanguageSelectionPopup.NameFor(SelectedLanguage);
    // Computed booleans used by XAML DataTriggers for unit button styling
    public bool IsMetricSelected => !IsImperialUnit;

    partial void OnIsImperialUnitChanged(bool value)
    {
        OnPropertyChanged(nameof(IsMetricSelected));

        if (value)
        {
            if (double.TryParse(HeightInput, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var h))
                HeightInput = Math.Round(UnitConversion.CmToInches(h), 1)
                    .ToString("F1", System.Globalization.CultureInfo.InvariantCulture);

            if (double.TryParse(WeightInput, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var w))
                WeightInput = Math.Round(UnitConversion.KgToLb(w), 1)
                    .ToString("F1", System.Globalization.CultureInfo.InvariantCulture);

        }
        else
        {
            if (double.TryParse(HeightInput, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var h))
                HeightInput = Math.Round(UnitConversion.InchesToCm(h), 1)
                    .ToString("F1", System.Globalization.CultureInfo.InvariantCulture);

            if (double.TryParse(WeightInput, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var w))
                WeightInput = Math.Round(UnitConversion.LbToKg(w), 1)
                    .ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
        }
        RefreshUnitLabels();
    }

    private void RefreshUnitLabels()
    {
        HeightLabel = IsImperialUnit
            ? LocalizationManager.Instance["Profile_Height_In"]
            : LocalizationManager.Instance["Profile_Height_Cm"];
        WeightLabel = IsImperialUnit
            ? LocalizationManager.Instance["Profile_Weight_Lb"]
            : LocalizationManager.Instance["Profile_Weight_Kg"];
    }

    [RelayCommand]
    private void SelectMetric() { if (IsImperialUnit) IsImperialUnit = false; }

    [RelayCommand]
    private void SelectImperial() { if (!IsImperialUnit) IsImperialUnit = true; }

    private bool CanContinue() => Nickname.Trim().Length >= 2;

    [RelayCommand(CanExecute = nameof(CanContinue))]
    private void Continue()
    {
        ValidationMessage = null;

        var ci = System.Globalization.CultureInfo.InvariantCulture;
        if (!double.TryParse(HeightInput, System.Globalization.NumberStyles.Any, ci, out var height) || height <= 0)
        {
            ValidationMessage = LocalizationManager.Instance["Profile_InvalidHeightWeight"];
            return;
        }

        if (!double.TryParse(WeightInput, System.Globalization.NumberStyles.Any, ci, out var weight) || weight <= 0)
        {
            ValidationMessage = LocalizationManager.Instance["Profile_InvalidHeightWeight"];
            return;
        }

        var preferredUnit = IsImperialUnit ? WeightUnit.Lb : WeightUnit.Kg;
        var heightCm = IsImperialUnit ? UnitConversion.InchesToCm(height) : height;
        var weightKg = IsImperialUnit ? UnitConversion.LbToKg(weight) : weight;

        _onboardingData.Nickname = Nickname.Trim();
        _onboardingData.Birthday = Birthday;
        _onboardingData.HeightCm = Math.Round(heightCm, 1);
        _onboardingData.WeightKg = Math.Round(weightKg, 2);
        _onboardingData.PreferredUnit = preferredUnit;
        _onboardingData.Gender = SelectedGenderDisplay switch
        {
            "Male" => Gender.Male,
            "Female" => Gender.Female,
            _ => Gender.PreferNotToSay
        };
        _onboardingData.Language = SelectedLanguage;

        ContinueRequested?.Invoke(this, EventArgs.Empty);
    }
}
