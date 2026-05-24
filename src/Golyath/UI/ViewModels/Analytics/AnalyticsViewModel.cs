using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.DTOs;
using Golyath.Application.Services;

namespace Golyath.UI.ViewModels.Analytics;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IAnalyticsService _analytics;
    private readonly ISuggestionsService _suggestions;

    private static readonly Color AccentColor  = Color.FromArgb("#FFD700");
    private static readonly Color AccentText   = Color.FromArgb("#111111");
    private static readonly Color DimColor     = Color.FromArgb("#888888");
    private static readonly Color TransparentC = Colors.Transparent;

    // ── Loading ──────────────────────────────────────────────────────────────

    [ObservableProperty] private bool _isBusy;

    // ── Period filter ─────────────────────────────────────────────────────────

    public static readonly string[] Periods = ["4W", "3M", "6M", "ALL"];

    [ObservableProperty] private string _selectedPeriod = "4W";

    public string PeriodLabel => SelectedPeriod switch
    {
        "4W"  => "LAST 4 WEEKS",
        "3M"  => "LAST 3 MONTHS",
        "6M"  => "LAST 6 MONTHS",
        _     => "ALL TIME"
    };

    public Color Period4WBg    => SelectedPeriod == "4W"  ? AccentColor : TransparentC;
    public Color Period3MBg    => SelectedPeriod == "3M"  ? AccentColor : TransparentC;
    public Color Period6MBg    => SelectedPeriod == "6M"  ? AccentColor : TransparentC;
    public Color PeriodAllBg   => SelectedPeriod == "ALL" ? AccentColor : TransparentC;
    public Color Period4WText  => SelectedPeriod == "4W"  ? AccentText  : DimColor;
    public Color Period3MText  => SelectedPeriod == "3M"  ? AccentText  : DimColor;
    public Color Period6MText  => SelectedPeriod == "6M"  ? AccentText  : DimColor;
    public Color PeriodAllText => SelectedPeriod == "ALL" ? AccentText  : DimColor;

    // ── 1RM Trend chart ───────────────────────────────────────────────────────

    [ObservableProperty] private IReadOnlyList<ExerciseOption> _exerciseOptions = [];
    [ObservableProperty] private ExerciseOption? _selectedExercise;
    [ObservableProperty] private IReadOnlyList<StrengthPoint> _strengthPoints = [];
    [ObservableProperty] private string _strengthExerciseName = string.Empty;
    [ObservableProperty] private bool _hasStrengthData;

    // ── Muscle balance ────────────────────────────────────────────────────────

    [ObservableProperty] private IReadOnlyList<MuscleBalanceItem> _muscleBalance = [];
    [ObservableProperty] private IReadOnlyDictionary<string, double> _muscleWeights
        = new Dictionary<string, double>();
    [ObservableProperty] private bool _isFrontView = true;

    // ── Gauges ────────────────────────────────────────────────────────────────

    [ObservableProperty] private int _recoveryScore;
    [ObservableProperty] private int _intensityScore;

    public double RecoveryGaugeValue  => RecoveryScore  / 100.0;
    public double IntensityGaugeValue => IntensityScore / 100.0;

    public Color RecoveryGaugeColor => AccentColor;  // always gold

    public Color IntensityGaugeColor => IntensityScore switch
    {
        > 85 => Color.FromArgb("#FF4444"),   // red  – overtraining risk
        > 70 => Color.FromArgb("#FFA500"),   // amber – high
        _    => Color.FromArgb("#44CC88")    // green – healthy
    };

    // ── Main Finding ──────────────────────────────────────────────────────────

    [ObservableProperty] private TrainingSuggestion? _mainFinding;

    public bool HasMainFinding => MainFinding is not null;

    public string? MainFindingCta => MainFinding?.Type switch
    {
        SuggestionType.MuscleImbalance    => "ADD PULL SESSION ›",
        SuggestionType.UndertrainedMuscle => "TRAIN NOW ›",
        _                                 => null
    };
    public bool HasMainFindingCta => MainFindingCta is not null;

    // ── All insights ──────────────────────────────────────────────────────────

    [ObservableProperty] private IReadOnlyList<TrainingSuggestion> _allInsights = [];
    public bool HasInsights => AllInsights.Count > 0;

    // ── Construction ──────────────────────────────────────────────────────────

    public AnalyticsViewModel(IAnalyticsService analytics, ISuggestionsService suggestions)
    {
        _analytics   = analytics;
        _suggestions = suggestions;
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var exercises = await _analytics.GetExercisesWithHistoryAsync();
            ExerciseOptions = exercises;

            if (SelectedExercise is null && exercises.Count > 0)
                SelectedExercise = exercises[0];

            await RefreshAllAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SelectPeriodAsync(string period)
    {
        SelectedPeriod = period;
        await RefreshAllAsync();
    }

    [RelayCommand]
    private async Task SelectExerciseAsync()
    {
        await LoadStrengthAsync();
    }

    [RelayCommand]
    private void ToggleFigureView()
    {
        IsFrontView = !IsFrontView;
    }

    [RelayCommand]
    private async Task NavigateToTrainAsync()
    {
        await Shell.Current.GoToAsync("//WorkoutTemplatesPage");
    }

    // ── Partial property-changed hooks ────────────────────────────────────────

    partial void OnSelectedPeriodChanged(string value)
    {
        OnPropertyChanged(nameof(PeriodLabel));
        OnPropertyChanged(nameof(Period4WBg));
        OnPropertyChanged(nameof(Period3MBg));
        OnPropertyChanged(nameof(Period6MBg));
        OnPropertyChanged(nameof(PeriodAllBg));
        OnPropertyChanged(nameof(Period4WText));
        OnPropertyChanged(nameof(Period3MText));
        OnPropertyChanged(nameof(Period6MText));
        OnPropertyChanged(nameof(PeriodAllText));
    }

    partial void OnSelectedExerciseChanged(ExerciseOption? value)
    {
        if (value is not null)
            _ = LoadStrengthAsync();
    }

    partial void OnRecoveryScoreChanged(int value)
    {
        OnPropertyChanged(nameof(RecoveryGaugeValue));
    }

    partial void OnIntensityScoreChanged(int value)
    {
        OnPropertyChanged(nameof(IntensityGaugeValue));
        OnPropertyChanged(nameof(IntensityGaugeColor));
    }

    partial void OnMainFindingChanged(TrainingSuggestion? value)
    {
        OnPropertyChanged(nameof(HasMainFinding));
        OnPropertyChanged(nameof(MainFindingCta));
        OnPropertyChanged(nameof(HasMainFindingCta));
    }

    partial void OnAllInsightsChanged(IReadOnlyList<TrainingSuggestion> value)
    {
        OnPropertyChanged(nameof(HasInsights));
    }

    partial void OnMuscleBalanceChanged(IReadOnlyList<MuscleBalanceItem> value)
    {
        var dict = value.ToDictionary(m => m.Label, m => m.Fraction);
        // Derive a combined "Arms" weight for the body map figure
        double biceps  = dict.GetValueOrDefault("Biceps");
        double triceps = dict.GetValueOrDefault("Triceps");
        dict["Arms"] = Math.Max(biceps, triceps);
        MuscleWeights = dict;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private DateTime FromDate() => SelectedPeriod switch
    {
        "4W"  => DateTime.UtcNow.AddDays(-28),
        "3M"  => DateTime.UtcNow.AddDays(-90),
        "6M"  => DateTime.UtcNow.AddDays(-180),
        _     => DateTime.MinValue
    };

    private async Task RefreshAllAsync()
    {
        var from = FromDate();
        await Task.WhenAll(
            LoadStrengthAsync(from),
            LoadMuscleBalanceAsync(from),
            LoadGaugesAsync(from),
            LoadInsightsAsync());
    }

    private async Task LoadStrengthAsync(DateTime? from = null)
    {
        if (SelectedExercise is null) return;

        var result = await _analytics.GetStrengthProgressionAsync(
            SelectedExercise.Id, from ?? FromDate());

        if (result is null) return;

        StrengthExerciseName = result.ExerciseName;
        StrengthPoints       = result.Points;
        HasStrengthData      = result.Points.Count > 0;
    }

    private async Task LoadMuscleBalanceAsync(DateTime from)
    {
        MuscleBalance = await _analytics.GetMuscleBalanceAsync(from);
    }

    private async Task LoadGaugesAsync(DateTime from)
    {
        var recoveryTask  = _analytics.GetRecoveryScoreAsync();
        var intensityTask = _analytics.GetIntensityScoreAsync(from);

        await Task.WhenAll(recoveryTask, intensityTask);

        RecoveryScore  = recoveryTask.Result;
        IntensityScore = intensityTask.Result;
    }

    private async Task LoadInsightsAsync()
    {
        var suggestions = await _suggestions.GetSuggestionsAsync();
        AllInsights = suggestions;
        MainFinding = suggestions
            .OrderByDescending(s => (int)s.Priority)
            .FirstOrDefault();
    }
}
