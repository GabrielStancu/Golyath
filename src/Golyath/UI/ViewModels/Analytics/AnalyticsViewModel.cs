using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.DTOs;
using Golyath.Application.Services;

namespace Golyath.UI.ViewModels.Analytics;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IAnalyticsService _analytics;

    private static readonly Color AccentColor   = Color.FromArgb("#FFD700");
    private static readonly Color AccentText    = Color.FromArgb("#111111");
    private static readonly Color DimColor      = Color.FromArgb("#888888");
    private static readonly Color TransparentC  = Colors.Transparent;

    // ── Loading ──────────────────────────────────────────────────────────────

    [ObservableProperty] private bool _isBusy;

    // ── Active metric tab ────────────────────────────────────────────────────

    [ObservableProperty] private int _selectedMetricIndex;   // 0=Strength 1=Volume 2=Muscles

    public bool IsStrengthVisible => SelectedMetricIndex == 0;
    public bool IsVolumeVisible   => SelectedMetricIndex == 1;
    public bool IsMusclesVisible  => SelectedMetricIndex == 2;

    // Tab accent colors surfaced for XAML binding
    public Color StrengthTabBg   => SelectedMetricIndex == 0 ? AccentColor : TransparentC;
    public Color VolumeTabBg     => SelectedMetricIndex == 1 ? AccentColor : TransparentC;
    public Color MusclesTabBg    => SelectedMetricIndex == 2 ? AccentColor : TransparentC;
    public Color StrengthTabText => SelectedMetricIndex == 0 ? AccentText  : DimColor;
    public Color VolumeTabText   => SelectedMetricIndex == 1 ? AccentText  : DimColor;
    public Color MusclesTabText  => SelectedMetricIndex == 2 ? AccentText  : DimColor;

    // ── Period filter ────────────────────────────────────────────────────────

    public static readonly string[] PeriodOptions =
        ["4 Weeks", "3 Months", "6 Months", "All Time"];

    [ObservableProperty] private string _selectedPeriod = PeriodOptions[0];

    // ── Strength tab ─────────────────────────────────────────────────────────

    [ObservableProperty] private IReadOnlyList<ExerciseOption> _exerciseOptions = [];
    [ObservableProperty] private ExerciseOption? _selectedExercise;
    [ObservableProperty] private IReadOnlyList<StrengthPoint> _strengthPoints = [];
    [ObservableProperty] private string _strengthExerciseName = string.Empty;
    [ObservableProperty] private bool _hasStrengthData;

    // ── Volume tab ───────────────────────────────────────────────────────────

    [ObservableProperty] private IReadOnlyList<VolumePoint> _volumePoints = [];
    [ObservableProperty] private bool _hasVolumeData;

    // ── Muscles tab ──────────────────────────────────────────────────────────

    [ObservableProperty] private IReadOnlyList<MuscleGroupVolume> _muscleDistribution = [];
    [ObservableProperty] private bool _hasMuscleData;

    /// <summary>Dynamic height so the horizontal bar chart grows with the number of muscle groups.</summary>
    public double MuscleChartHeight => Math.Max(120, MuscleDistribution.Count * 30 + 12);

    // ── Construction ─────────────────────────────────────────────────────────

    public AnalyticsViewModel(IAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var exercises = await _analytics.GetExercisesWithHistoryAsync();
            ExerciseOptions = exercises;

            if (SelectedExercise is null && exercises.Count > 0)
                SelectedExercise = exercises[0];

            await RefreshChartsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SelectMetricAsync(string metricIndex)
    {
        if (int.TryParse(metricIndex, out int idx))
            SelectedMetricIndex = idx;

        OnPropertyChanged(nameof(IsStrengthVisible));
        OnPropertyChanged(nameof(IsVolumeVisible));
        OnPropertyChanged(nameof(IsMusclesVisible));

        await RefreshChartsAsync();
    }

    [RelayCommand]
    private async Task ApplyPeriodAsync()
    {
        await RefreshChartsAsync();
    }

    [RelayCommand]
    private async Task SelectExerciseAsync()
    {
        if (SelectedMetricIndex == 0)
            await LoadStrengthAsync();
    }

    // ── Partial property changed hooks ───────────────────────────────────────

    partial void OnSelectedMetricIndexChanged(int value)
    {
        OnPropertyChanged(nameof(IsStrengthVisible));
        OnPropertyChanged(nameof(IsVolumeVisible));
        OnPropertyChanged(nameof(IsMusclesVisible));
        OnPropertyChanged(nameof(StrengthTabBg));
        OnPropertyChanged(nameof(VolumeTabBg));
        OnPropertyChanged(nameof(MusclesTabBg));
        OnPropertyChanged(nameof(StrengthTabText));
        OnPropertyChanged(nameof(VolumeTabText));
        OnPropertyChanged(nameof(MusclesTabText));
    }

    partial void OnMuscleDistributionChanged(IReadOnlyList<MuscleGroupVolume> value)
    {
        OnPropertyChanged(nameof(MuscleChartHeight));
    }

    partial void OnSelectedExerciseChanged(ExerciseOption? value)
    {
        if (value is not null && SelectedMetricIndex == 0)
        {
            // Fire-and-forget; the UI is already unblocked by IsBusy
            _ = LoadStrengthAsync();
        }
    }

    partial void OnSelectedPeriodChanged(string value)
    {
        _ = RefreshChartsAsync();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private DateTime FromDate()
    {
        var now = DateTime.UtcNow;
        return SelectedPeriod switch
        {
            "4 Weeks"   => now.AddDays(-28),
            "3 Months"  => now.AddDays(-90),
            "6 Months"  => now.AddDays(-180),
            _           => DateTime.MinValue   // All Time
        };
    }

    private async Task RefreshChartsAsync()
    {
        var from = FromDate();
        switch (SelectedMetricIndex)
        {
            case 0: await LoadStrengthAsync(from); break;
            case 1: await LoadVolumeAsync(from);   break;
            case 2: await LoadMusclesAsync(from);  break;
        }
    }

    private async Task LoadStrengthAsync(DateTime? from = null)
    {
        if (SelectedExercise is null) return;

        var result = await _analytics.GetStrengthProgressionAsync(
            SelectedExercise.Id,
            from ?? FromDate());

        if (result is null) return;

        StrengthExerciseName = result.ExerciseName;
        StrengthPoints       = result.Points;
        HasStrengthData      = result.Points.Count > 0;
    }

    private async Task LoadVolumeAsync(DateTime? from = null)
    {
        var points  = await _analytics.GetWeeklyVolumeAsync(from ?? FromDate());
        VolumePoints  = points;
        HasVolumeData = points.Count > 0;
    }

    private async Task LoadMusclesAsync(DateTime? from = null)
    {
        var dist      = await _analytics.GetMuscleGroupDistributionAsync(from ?? FromDate());
        MuscleDistribution = dist;
        HasMuscleData      = dist.Count > 0;
    }
}
