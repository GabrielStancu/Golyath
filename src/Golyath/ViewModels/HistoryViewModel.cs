using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Models;
using Golyath.Services;
using System.Collections.ObjectModel;

namespace Golyath.ViewModels;

public record SessionSummaryItem(
    WorkoutSession Session,
    string DateLabel,
    string TimeLabel,
    string DurationLabel,
    string ExerciseSummary);

public partial class HistoryViewModel : BaseViewModel
{
    private readonly IWorkoutService _workoutService;

    [ObservableProperty]
    private ObservableCollection<SessionSummaryItem> _sessionSummaries = [];

    [ObservableProperty]
    private bool _isEmpty;

    public HistoryViewModel(IWorkoutService workoutService)
    {
        _workoutService = workoutService;
        Title = "History";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var sessions = await _workoutService.GetSessionsAsync();
            IsEmpty = sessions.Count == 0;

            var summaries = new List<SessionSummaryItem>();
            foreach (var session in sessions)
            {
                var names = await _workoutService.GetExerciseNamesForSessionAsync(session.Id);
                var exerciseSummary = names.Count > 0
                    ? string.Join(" · ", names.Take(3)) + (names.Count > 3 ? $"  +{names.Count - 3}" : "")
                    : "No exercises recorded";

                summaries.Add(new SessionSummaryItem(
                    session,
                    session.StartedAt.ToLocalTime().ToString("ddd, MMM d"),
                    session.StartedAt.ToLocalTime().ToString("h:mm tt"),
                    $"{(int)session.Duration.TotalMinutes} min",
                    exerciseSummary));
            }
            SessionSummaries = new ObservableCollection<SessionSummaryItem>(summaries);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectSessionAsync(SessionSummaryItem item)
    {
        await Shell.Current.GoToAsync($"session-detail?sessionId={item.Session.Id}");
    }
}
