using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.Localization;
using Golyath.Application.Services;
using Golyath.Core.Enums;
using Golyath.Core.Utilities;

namespace Golyath.UI.ViewModels.Profile;

public partial class EditProfileViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IThemeService _themeService;
    private readonly IDataPortabilityService _dataPortabilityService;

    public EditProfileViewModel(
        IUserService userService,
        IThemeService themeService,
        IDataPortabilityService dataPortabilityService)
    {
        _userService = userService;
        _themeService = themeService;
        _dataPortabilityService = dataPortabilityService;

        // When language changes live, refresh unit labels
        LocalizationManager.Instance.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "Item[]")
                RefreshUnitLabels();
        };
    }

    public event EventHandler? SaveCompleted;

    [ObservableProperty] private string _nickname = string.Empty;
    [ObservableProperty] private DateTime _birthday = DateTime.Today.AddYears(-25);

    public DateTime MaxBirthday { get; } = DateTime.Today.AddYears(-10);

    [ObservableProperty] private string _heightInput = string.Empty;
    [ObservableProperty] private string _weightInput = string.Empty;
    [ObservableProperty] private string _heightLabel = LocalizationManager.Instance["Profile_HeightLabel_Cm"];
    [ObservableProperty] private string _weightLabel = LocalizationManager.Instance["Profile_WeightLabel_Kg"];
    [ObservableProperty] private bool _isImperialUnit;

    public bool IsMetricSelected => !IsImperialUnit;

    [ObservableProperty] private string _selectedGenderDisplay = "Prefer not to say";
    [ObservableProperty] private string _selectedGoalDisplay = "Balanced";
    [ObservableProperty] private bool _isDarkMode;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLanguageFlag))]
    [NotifyPropertyChangedFor(nameof(SelectedLanguageName))]
    private AppLanguage _selectedLanguage = AppLanguage.English;

    public string SelectedLanguageFlag => UI.Controls.LanguageSelectionPopup.FlagFor(SelectedLanguage);
    public string SelectedLanguageName => UI.Controls.LanguageSelectionPopup.NameFor(SelectedLanguage);
    private bool _isInitializing;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSaveMessage))]
    private string? _saveMessage;

    public bool HasSaveMessage => !string.IsNullOrEmpty(SaveMessage);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    public IReadOnlyList<string> GenderOptions { get; } =
        ["Male", "Female", "Prefer not to say"];

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
        }
        else
        {
            if (double.TryParse(HeightInput, System.Globalization.NumberStyles.Any, ci, out var h))
                HeightInput = Math.Round(UnitConversion.InchesToCm(h), 1).ToString("F1", ci);
            if (double.TryParse(WeightInput, System.Globalization.NumberStyles.Any, ci, out var w))
                WeightInput = Math.Round(UnitConversion.LbToKg(w), 1).ToString("F1", ci);
        }
        RefreshUnitLabels();
    }

    private void RefreshUnitLabels()
    {
        HeightLabel = IsImperialUnit
            ? LocalizationManager.Instance["Profile_HeightLabel_In"]
            : LocalizationManager.Instance["Profile_HeightLabel_Cm"];
        WeightLabel = IsImperialUnit
            ? LocalizationManager.Instance["Profile_WeightLabel_Lb"]
            : LocalizationManager.Instance["Profile_WeightLabel_Kg"];
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        if (_isInitializing) return;
        _themeService.ApplyTheme(value ? AppTheme.Dark : AppTheme.Light);
    }

    [RelayCommand]
    private void SelectMetric() { if (IsImperialUnit) IsImperialUnit = false; }

    [RelayCommand]
    private void SelectImperial() { if (!IsImperialUnit) IsImperialUnit = true; }

    public async Task InitializeAsync()
    {
        _isInitializing = true;
        try
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user is null) return;

            Nickname = user.Nickname;
            Birthday = user.Birthday;
            IsImperialUnit = user.PreferredUnit == WeightUnit.Lb;

            var preferred = _themeService.GetPreferredTheme();
            IsDarkMode = preferred == AppTheme.Dark
                || (preferred == AppTheme.Unspecified
                    && Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Dark);

            var ci = System.Globalization.CultureInfo.InvariantCulture;

            // Set display values (conversion already handled by OnIsImperialUnitChanged side-effect)
            // Set raw metric values first; the property change will convert if needed
            HeightInput = user.HeightCm.ToString("F1", ci);
            WeightInput = user.WeightKg.ToString("F2", ci);

            SelectedGenderDisplay = user.Gender switch
            {
                Gender.Male   => "Male",
                Gender.Female => "Female",
                _             => "Prefer not to say"
            };

            SelectedGoalDisplay = user.FitnessGoal switch
            {
                FitnessGoal.Strength    => "Strength",
                FitnessGoal.Hypertrophy => "Hypertrophy",
                FitnessGoal.FatLoss     => "Fat Loss",
                _                       => "Balanced"
            };

            SelectedLanguage = user.Language;
        }
        finally
        {
            _isInitializing = false;
        }
    }

    // ─── Data portability ─────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDataStatus))]
    private string _dataStatusMessage = string.Empty;

    public bool HasDataStatus => !string.IsNullOrEmpty(DataStatusMessage);

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task ExportDataAsync()
    {
        IsBusy = true;
        DataStatusMessage = string.Empty;
        ExportDataCommand.NotifyCanExecuteChanged();
        ImportDataCommand.NotifyCanExecuteChanged();
        try
        {
            var json = await _dataPortabilityService.ExportToJsonAsync();
            var fileName = $"golyath-backup-{DateTime.Now:yyyy-MM-dd}.json";

#if ANDROID
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
            {
                var values = new Android.Content.ContentValues();
                values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "application/json");
                values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath,
                    Android.OS.Environment.DirectoryDownloads);
                var resolver = Android.App.Application.Context.ContentResolver!;
                var uri = resolver.Insert(Android.Provider.MediaStore.Downloads.ExternalContentUri!, values)
                    ?? throw new InvalidOperationException("MediaStore failed to create the download entry.");
                using var os = resolver.OpenOutputStream(uri)
                    ?? throw new InvalidOperationException("Could not open output stream for download.");
                await os.WriteAsync(bytes);
                DataStatusMessage = $"Backup saved to Downloads/{fileName}";
            }
            else
            {
                var downloadsDir = Android.OS.Environment
                    .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)!
                    .AbsolutePath;
                var filePath = Path.Combine(downloadsDir, fileName);
                await File.WriteAllBytesAsync(filePath, bytes);
                DataStatusMessage = $"Backup saved to Downloads/{fileName}";
            }
#else
            var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(tempPath, json);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Save Golyath Backup",
                File = new ShareFile(tempPath, "application/json"),
            });
            DataStatusMessage = "Backup export initiated.";
#endif
        }
        catch (Exception ex)
        {
            DataStatusMessage = $"Export failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            ExportDataCommand.NotifyCanExecuteChanged();
            ImportDataCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task ImportDataAsync()
    {
        var fileResult = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select Golyath backup",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".json"] },
                { DevicePlatform.Android, ["*/*"] },
                { DevicePlatform.iOS, ["public.json", "public.text"] },
                { DevicePlatform.MacCatalyst, ["public.json", "public.text"] }
            })
        });

        if (fileResult is null) return;

        IsBusy = true;
        DataStatusMessage = string.Empty;
        ExportDataCommand.NotifyCanExecuteChanged();
        ImportDataCommand.NotifyCanExecuteChanged();
        try
        {
            using var stream = await fileResult.OpenReadAsync();
            var result = await _dataPortabilityService.ImportFromStreamAsync(stream);
            DataStatusMessage = result.Success
                ? $"Import complete. {result.ItemsImported} items added."
                : $"Import failed: {result.Message}";
        }
        catch (Exception ex)
        {
            DataStatusMessage = $"Import failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
            ExportDataCommand.NotifyCanExecuteChanged();
            ImportDataCommand.NotifyCanExecuteChanged();
        }
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
                SaveMessage = LocalizationManager.Instance["Profile_InvalidHeightWeight"];
                return;
            }

            var heightCm = IsImperialUnit ? UnitConversion.InchesToCm(height) : height;
            var weightKg = IsImperialUnit ? UnitConversion.LbToKg(weight) : weight;

            var gender = SelectedGenderDisplay switch
            {
                "Male"   => Gender.Male,
                "Female" => Gender.Female,
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
            user.Language    = SelectedLanguage;

            await _userService.UpdateUserAsync(user);
            LocalizationManager.Instance.SetLanguage(SelectedLanguage);
            SaveMessage = LocalizationManager.Instance["Profile_Saved"];
            SaveCompleted?.Invoke(this, EventArgs.Empty);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
