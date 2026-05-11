using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.Services;
using Golyath.Core.Enums;

namespace Golyath.UI.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IDataPortabilityService _dataPortabilityService;
    private readonly IUserService _userService;
    private readonly ISettingsService _settingsService;
    private Core.Entities.User? _currentUser;

    // (label, seconds) pairs shown in the rest-timer picker
    public static readonly IReadOnlyList<(string Label, int Seconds)> RestTimerOptions =
    [
        ("30 sec", 30),
        ("45 sec", 45),
        ("1 min", 60),
        ("1 min 30 sec", 90),
        ("2 min", 120),
        ("3 min", 180),
        ("5 min", 300),
    ];

    public IReadOnlyList<string> RestTimerLabels { get; } =
        RestTimerOptions.Select(o => o.Label).ToList();

    public SettingsViewModel(
        IDataPortabilityService dataPortabilityService,
        IUserService userService,
        ISettingsService settingsService)
    {
        _dataPortabilityService = dataPortabilityService;
        _userService = userService;
        _settingsService = settingsService;

        var savedSeconds = _settingsService.GetDefaultRestSeconds();
        var idx = RestTimerOptions.ToList().FindIndex(o => o.Seconds == savedSeconds);
        _selectedRestTimerIndex = idx >= 0 ? idx : 3; // default index for 90 s
    }

    public async Task InitializeAsync()
    {
        _currentUser = await _userService.GetCurrentUserAsync();
        if (_currentUser is not null)
        {
            _isImperialUnit = _currentUser.PreferredUnit == WeightUnit.Lb;
            OnPropertyChanged(nameof(IsImperialUnit));
            OnPropertyChanged(nameof(IsMetricSelected));
        }
    }

    // ─── Preferences ─────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMetricSelected))]
    private bool _isImperialUnit;

    public bool IsMetricSelected => !IsImperialUnit;

    [ObservableProperty]
    private int _selectedRestTimerIndex;

    partial void OnIsImperialUnitChanged(bool value)
    {
        if (_currentUser is null) return;
        _currentUser.PreferredUnit = value ? WeightUnit.Lb : WeightUnit.Kg;
        _ = _userService.UpdateUserAsync(_currentUser);
    }

    partial void OnSelectedRestTimerIndexChanged(int value)
    {
        if (value < 0 || value >= RestTimerOptions.Count) return;
        _settingsService.SetDefaultRestSeconds(RestTimerOptions[value].Seconds);
    }

    [RelayCommand]
    private void SelectMetric() => IsImperialUnit = false;

    [RelayCommand]
    private void SelectImperial() => IsImperialUnit = true;

    // ─── Shared state ─────────────────────────────────────────────────────────

    [ObservableProperty]
    bool _isBusy;

    [ObservableProperty]
    string _statusMessage = string.Empty;

    [ObservableProperty]
    bool _hasStatus;

    public bool IsNotBusy => !IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        ExportDataCommand.NotifyCanExecuteChanged();
        ImportDataCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsNotBusy));
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task ExportDataAsync()
    {
        IsBusy = true;
        StatusMessage = string.Empty;
        HasStatus = false;
        try
        {
            var json = await _dataPortabilityService.ExportToJsonAsync();
            var fileName = $"golyath-backup-{DateTime.Now:yyyy-MM-dd}.json";

#if ANDROID
            // Write directly to the device Downloads folder via MediaStore (API 29+, no
            // permission required). This avoids routing through Google Drive's share intent,
            // which auto-converts text uploads to Google Docs and serves them back as PDF.
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
                StatusMessage = $"Backup saved to Downloads/{fileName}";
                HasStatus = true;
            }
            else
            {
                // API 21–28 fallback: write directly to the external Downloads directory.
                // Requires WRITE_EXTERNAL_STORAGE (declared in AndroidManifest with maxSdkVersion=28).
                var downloadsDir = Android.OS.Environment
                    .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)!
                    .AbsolutePath;
                var filePath = Path.Combine(downloadsDir, fileName);
                await File.WriteAllBytesAsync(filePath, bytes);
                StatusMessage = $"Backup saved to Downloads/{fileName}";
                HasStatus = true;
            }
#else
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            var saveResult = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);
            if (saveResult.IsSuccessful)
            {
                StatusMessage = "Backup saved successfully.";
                HasStatus = true;
            }
            else if (saveResult.Exception is not null)
            {
                StatusMessage = $"Export failed: {saveResult.Exception.Message}";
                HasStatus = true;
            }
            // IsSuccessful=false with no exception means the user cancelled — no message needed
#endif
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
            HasStatus = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task ImportDataAsync()
    {
        var fileResult = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select Golyath backup",
            // Use broad types on Android — files from WhatsApp/Drive/etc. may not carry
            // the application/json MIME type and would be invisible with a strict filter.
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
        StatusMessage = string.Empty;
        HasStatus = false;
        try
        {
            // Pass the stream directly — avoids any encoding conversion that
            // StreamReader or certain content:// providers might silently apply.
            using var stream = await fileResult.OpenReadAsync();
            var result = await _dataPortabilityService.ImportFromStreamAsync(stream);
            StatusMessage = result.Success
                ? $"Import complete. {result.ItemsImported} items added."
                : $"Import failed: {result.Message}";
            HasStatus = true;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import failed: {ex.Message}";
            HasStatus = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
