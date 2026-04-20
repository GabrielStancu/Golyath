using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Models;
using Golyath.Services;
using System.Collections.ObjectModel;

namespace Golyath.ViewModels;

/// <summary>Groups sets by exercise for the session detail view.</summary>
public class ExerciseSetGroup : ObservableCollection<WorkoutSet>
{
    public string ExerciseName { get; }
    public int TotalSets => Count;
    public double TotalVolume => this.Sum(s => s.Volume);

    public ExerciseSetGroup(string exerciseName, IEnumerable<WorkoutSet> sets)
        : base(sets)
    {
        ExerciseName = exerciseName;
    }
}

[QueryProperty(nameof(SessionId), "sessionId")]
public partial class SessionDetailViewModel : BaseViewModel
{
    private readonly IWorkoutService _workoutService;
    private readonly IExerciseService _exerciseService;

    [ObservableProperty]
    private int _sessionId;

    [ObservableProperty]
    private WorkoutSession? _session;

    [ObservableProperty]
    private ObservableCollection<ExerciseSetGroup> _exerciseGroups = [];

    [ObservableProperty]
    private string _totalVolume = "0 kg";

    [ObservableProperty]
    private string _duration = "0 min";

    [ObservableProperty]
    private string _dateLabel = string.Empty;

    public SessionDetailViewModel(IWorkoutService workoutService, IExerciseService exerciseService)
    {
        _workoutService = workoutService;
        _exerciseService = exerciseService;
    }

    partial void OnSessionIdChanged(int value)
    {
        if (value > 0) LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy || SessionId <= 0) return;
        IsBusy = true;
        try
        {
            Session = await _workoutService.GetSessionByIdAsync(SessionId);
            if (Session is null) return;

            DateLabel = Session.StartedAt.ToLocalTime().ToString("dddd, MMMM d, yyyy");
            Duration = $"{(int)Session.Duration.TotalMinutes} min";

            var sets = await _workoutService.GetSetsForSessionAsync(SessionId);
            var allExercises = await _exerciseService.GetAllExercisesAsync();
            var exerciseMap = allExercises.ToDictionary(e => e.Id, e => e.Name);

            var groups = sets
                .GroupBy(s => s.ExerciseId)
                .Select(g => new ExerciseSetGroup(
                    exerciseMap.GetValueOrDefault(g.Key, "Unknown"),
                    g.OrderBy(s => s.SetNumber)))
                .ToList();

            ExerciseGroups = new ObservableCollection<ExerciseSetGroup>(groups);

            double total = sets.Sum(s => s.Volume);
            TotalVolume = $"{total:N0} kg";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
