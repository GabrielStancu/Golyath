using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Models;
using Golyath.Services;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Golyath.ViewModels;

[QueryProperty(nameof(ExerciseId), "exerciseId")]
public partial class ExerciseDetailViewModel : BaseViewModel
{
    private readonly IExerciseService _exerciseService;

    [ObservableProperty] private int _exerciseId;
    [ObservableProperty] private Exercise? _exercise;
    [ObservableProperty] private string _primaryMuscleName = string.Empty;
    [ObservableProperty] private string _equipmentName = string.Empty;
    [ObservableProperty] private ObservableCollection<string> _secondaryMuscleNames = [];
    [ObservableProperty] private ImageSource? _currentExerciseImage;
    [ObservableProperty] private ObservableCollection<string> _exerciseInstructions = [];
    [ObservableProperty] private string _exerciseLevel = string.Empty;
    [ObservableProperty] private string _exerciseForce = string.Empty;
    [ObservableProperty] private string _exerciseMechanic = string.Empty;

    public bool HasSecondaryMuscles => SecondaryMuscleNames.Count > 0;
    public bool HasDescription => !string.IsNullOrWhiteSpace(Exercise?.Description);
    public bool HasInstructions => ExerciseInstructions.Count > 0;
    public bool HasExtraInfo => !string.IsNullOrEmpty(ExerciseLevel);

    private CancellationTokenSource? _animationCts;
    private byte[]? _frame0Bytes;
    private byte[]? _frame1Bytes;

    public ExerciseDetailViewModel(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    partial void OnExerciseIdChanged(int value)
    {
        if (value > 0)
            LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy || ExerciseId <= 0) return;
        IsBusy = true;
        try
        {
            Exercise = await _exerciseService.GetExerciseByIdAsync(ExerciseId);
            if (Exercise is null) return;

            Title = Exercise.Name;

            var muscleGroups = await _exerciseService.GetMuscleGroupsAsync();
            var equipment = await _exerciseService.GetEquipmentAsync();
            var links = await _exerciseService.GetMuscleLinksForExerciseAsync(ExerciseId);

            var primary = muscleGroups.FirstOrDefault(m => m.Id == Exercise.PrimaryMuscleGroupId);
            PrimaryMuscleName = primary?.DisplayName ?? "Unknown";

            var eq = equipment.FirstOrDefault(e => e.Id == Exercise.EquipmentId);
            EquipmentName = eq?.Name ?? "Unknown";

            var secondaryIds = links
                .Where(l => l.Role == MuscleRole.Secondary)
                .Select(l => l.MuscleGroupId);

            var secondaryNames = muscleGroups
                .Where(m => secondaryIds.Contains(m.Id))
                .Select(m => m.DisplayName);

            SecondaryMuscleNames = new ObservableCollection<string>(secondaryNames);
            OnPropertyChanged(nameof(HasSecondaryMuscles));
            OnPropertyChanged(nameof(HasDescription));

            await LoadExerciseInfoAsync();

            _frame0Bytes = await LoadImageBytesAsync(Exercise.ImageFrame0);
            _frame1Bytes = await LoadImageBytesAsync(Exercise.ImageFrame1);
            StartAnimation();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadExerciseInfoAsync()
    {
        if (Exercise is null || string.IsNullOrEmpty(Exercise.ImagePath)) return;
        try
        {
            var path = $"exercises/{Exercise.ImagePath}/info.json";
            await using var stream = await FileSystem.OpenAppPackageFileAsync(path);
            var info = await JsonSerializer.DeserializeAsync<ExerciseInfo>(
                stream, ExerciseInfoJsonContext.Default.ExerciseInfo);
            if (info is null) return;

            ExerciseLevel    = Capitalize(info.Level);
            ExerciseForce    = Capitalize(info.Force);
            ExerciseMechanic = Capitalize(info.Mechanic);

            if (info.Instructions is { Count: > 0 })
                ExerciseInstructions = new ObservableCollection<string>(info.Instructions);

            OnPropertyChanged(nameof(HasInstructions));
            OnPropertyChanged(nameof(HasExtraInfo));
        }
        catch { /* missing or malformed JSON — skip gracefully */ }
    }

    private static async Task<byte[]?> LoadImageBytesAsync(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(path);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch { return null; }
    }

    public void StartAnimation()
    {
        StopAnimation();
        if (_frame0Bytes is null && _frame1Bytes is null) return;
        _animationCts = new CancellationTokenSource();
        var ct = _animationCts.Token;
        _ = AnimateAsync(ct);
    }

    private async Task AnimateAsync(CancellationToken ct)
    {
        bool showFrame0 = true;
        while (!ct.IsCancellationRequested)
        {
            var bytes = showFrame0 ? _frame0Bytes : _frame1Bytes;
            if (bytes is not null)
            {
                var captured = bytes;
                MainThread.BeginInvokeOnMainThread(() =>
                    CurrentExerciseImage = ImageSource.FromStream(() => new MemoryStream(captured)));
            }
            showFrame0 = !showFrame0;
            try { await Task.Delay(800, ct); }
            catch (OperationCanceledException) { break; }
        }
    }

    public void StopAnimation()
    {
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = null;
    }

    private static string Capitalize(string? s) =>
        string.IsNullOrEmpty(s) ? string.Empty :
        char.ToUpperInvariant(s[0]) + s[1..];
}

// ── Minimal DTO for exercise info JSON ────────────────────────────────────────
internal sealed class ExerciseInfo
{
    [JsonPropertyName("level")]        public string?       Level        { get; init; }
    [JsonPropertyName("force")]        public string?       Force        { get; init; }
    [JsonPropertyName("mechanic")]     public string?       Mechanic     { get; init; }
    [JsonPropertyName("instructions")] public List<string>? Instructions { get; init; }
}

[JsonSerializable(typeof(ExerciseInfo))]
internal sealed partial class ExerciseInfoJsonContext : JsonSerializerContext { }
