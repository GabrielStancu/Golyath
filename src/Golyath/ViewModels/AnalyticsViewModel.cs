using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Charts;
using Golyath.Services;

namespace Golyath.ViewModels;

public partial class AnalyticsViewModel : BaseViewModel
{
    private readonly IAnalyticsService _analyticsService;

    // ── Tab state ────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVolumeTab))]
    [NotifyPropertyChangedFor(nameof(IsMusclesTab))]
    [NotifyPropertyChangedFor(nameof(IsFrequencyTab))]
    private string _activeTab = "volume";

    public bool IsVolumeTab    => _activeTab == "volume";
    public bool IsMusclesTab   => _activeTab == "muscles";
    public bool IsFrequencyTab => _activeTab == "frequency";

    // ── Charts ───────────────────────────────────────────────────────

    [ObservableProperty]
    private WeeklyVolumeChartDrawable _volumeChart = new();

    [ObservableProperty]
    private HorizontalBarChartDrawable _muscleChart = new();

    [ObservableProperty]
    private FrequencyChartDrawable _frequencyChart = new();

    // ── Muscle list & range ───────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMuscleSectionEmpty))]
    private ObservableCollection<MuscleVolume> _muscleItems = [];

    public bool IsMuscleSectionEmpty => _muscleItems.Count == 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MuscleRangeLabel))]
    private int _muscleRangeDays = 30;

    public string MuscleRangeLabel => $"Last {MuscleRangeDays} days";

    // ── Volume stats ─────────────────────────────────────────────────

    [ObservableProperty]
    private string _totalWeekVolume = "–";

    [ObservableProperty]
    private string _peakDayLabel = "–";

    [ObservableProperty]
    private string _activeDaysLabel = "–";

    // ── Loading ───────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isLoading;

    public AnalyticsViewModel(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
        Title = "Analytics";
    }

    // ── Tab commands ─────────────────────────────────────────────────

    [RelayCommand]
    private void SetVolumeTab()    => ActiveTab = "volume";

    [RelayCommand]
    private void SetMusclesTab()   => ActiveTab = "muscles";

    [RelayCommand]
    private void SetFrequencyTab() => ActiveTab = "frequency";

    // ── Muscle range cycling ─────────────────────────────────────────

    [RelayCommand]
    private async Task CycleMuscleRangeAsync()
    {
        MuscleRangeDays = MuscleRangeDays switch
        {
            7  => 30,
            30 => 90,
            _  => 7
        };
        await LoadMusclesAsync();
    }

    // ── Load ──────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            await Task.WhenAll(
                LoadVolumeAsync(),
                LoadMusclesAsync(),
                LoadFrequencyAsync());
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadVolumeAsync()
    {
        var days = await _analyticsService.GetCurrentWeekVolumeAsync();

        string[] dayLabels = ["M", "T", "W", "T", "F", "S", "S"];
        int todayIdx = ((int)DateTime.Today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        VolumeChart = new WeeklyVolumeChartDrawable
        {
            Labels = dayLabels,
            Values = days,
            HighlightIndex = todayIdx
        };

        float total = days.Sum();
        TotalWeekVolume = total >= 1000
            ? $"{total / 1000f:F1}t"
            : $"{total:F0} kg";

        int peakIdx = Array.IndexOf(days, days.Max());
        PeakDayLabel = days.Max() > 0
            ? $"{dayLabels[peakIdx]} ({days[peakIdx]:F0} kg)"
            : "–";

        int activeDays = days.Count(v => v > 0);
        ActiveDaysLabel = activeDays == 0 ? "None yet" : $"{activeDays}/7 days";
    }

    private async Task LoadMusclesAsync()
    {
        var muscles = await _analyticsService.GetMuscleDistributionAsync(MuscleRangeDays);

        MuscleChart = new HorizontalBarChartDrawable
        {
            Labels = muscles.Select(m => m.MuscleGroupName).ToArray(),
            Values = muscles.Select(m => (float)m.Fraction).ToArray()
        };

        MuscleItems = new ObservableCollection<MuscleVolume>(muscles);
    }

    private async Task LoadFrequencyAsync()
    {
        var freq = await _analyticsService.GetWorkoutFrequencyAsync(8);

        FrequencyChart = new FrequencyChartDrawable
        {
            Labels = freq.Select(f => f.WeekLabel).ToArray(),
            Values = freq.Select(f => f.SessionCount).ToArray(),
            HighlightIndex = freq.Count - 1
        };
    }
}

