using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Models;
using Golyath.Services;
using System.Collections.ObjectModel;

namespace Golyath.ViewModels;

/// <summary>Represents one exercise + its logged sets within an active workout session.</summary>
public partial class ActiveExerciseItem : ObservableObject
{
    public Exercise Exercise { get; init; } = null!;

    [ObservableProperty]
    private ObservableCollection<WorkoutSet> _sets = [];
}

public partial class ActiveWorkoutViewModel : BaseViewModel
{
    private readonly IWorkoutService _workoutService;
    private readonly IExerciseService _exerciseService;
    private readonly ITemplateService _templateService;

    private WorkoutSession? _currentSession;
    private System.Timers.Timer? _elapsedTimer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSessionIdle))]
    private bool _isSessionActive;

    public bool IsSessionIdle => !IsSessionActive;

    [ObservableProperty]
    private string _elapsedTime = "00:00";

    [ObservableProperty]
    private ObservableCollection<ActiveExerciseItem> _exerciseItems = [];

    [ObservableProperty]
    private ObservableCollection<Exercise> _availableExercises = [];

    [ObservableProperty]
    private bool _isPickerVisible;

    [ObservableProperty]
    private string _exercisePickerSearch = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Exercise> _filteredPickerExercises = [];

    // -- Template support --
    [ObservableProperty]
    private ObservableCollection<WorkoutTemplate> _templates = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTemplates))]
    private bool _templatesLoaded;

    public bool HasTemplates => _templates.Count > 0;

    public ActiveWorkoutViewModel(
        IWorkoutService workoutService,
        IExerciseService exerciseService,
        ITemplateService templateService)
    {
        _workoutService = workoutService;
        _exerciseService = exerciseService;
        _templateService = templateService;
        Title = "Workout";
    }

    [RelayCommand]
    public async Task LoadTemplatesAsync()
    {
        var list = await _templateService.GetTemplatesAsync();
        Templates = new ObservableCollection<WorkoutTemplate>(
            list.OrderByDescending(t => t.LastUsedAt ?? t.CreatedAt));
        TemplatesLoaded = true;
        OnPropertyChanged(nameof(HasTemplates));
    }

    // â”€â”€ Freeform start â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [RelayCommand]
    private async Task StartWorkoutAsync()
    {
        _currentSession = await _workoutService.StartSessionAsync();
        IsSessionActive = true;
        StartElapsedTimer();
    }

    // â”€â”€ Template start â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [RelayCommand]
    private async Task StartWorkoutFromTemplateAsync(WorkoutTemplate template)
    {
        _currentSession = await _workoutService.StartSessionAsync(template.Id);

        var details = await _templateService.GetEntryDetailsAsync(template.Id);
        foreach (var detail in details)
        {
            var item = new ActiveExerciseItem { Exercise = detail.Exercise };
            for (int s = 1; s <= detail.Entry.TargetSets; s++)
            {
                var set = await _workoutService.AddSetAsync(
                    _currentSession.Id,
                    detail.Exercise.Id,
                    s,
                    detail.Entry.TargetWeight,
                    detail.Entry.TargetReps,
                    tempo: detail.Entry.TargetTempo);
                item.Sets.Add(set);
            }
            ExerciseItems.Add(item);
        }

        // Update template's last-used timestamp
        template.LastUsedAt = DateTime.UtcNow;
        await _templateService.UpdateTemplateAsync(template);

        IsSessionActive = true;
        StartElapsedTimer();
    }

    // â”€â”€ Finish â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [RelayCommand]
    private async Task FinishWorkoutAsync()
    {
        if (_currentSession is null) return;

        _elapsedTimer?.Stop();
        _elapsedTimer?.Dispose();
        _elapsedTimer = null;

        await _workoutService.FinishSessionAsync(_currentSession);

        _currentSession = null;
        IsSessionActive = false;
        ElapsedTime = "00:00";
        ExerciseItems.Clear();
    }

    // â”€â”€ Exercise picker â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [RelayCommand]
    private async Task ShowExercisePickerAsync()
    {
        if (_availableExercises.Count == 0)
        {
            var all = await _exerciseService.GetAllExercisesAsync();
            _availableExercises = new ObservableCollection<Exercise>(all.OrderBy(e => e.Name));
        }
        FilteredPickerExercises = new ObservableCollection<Exercise>(_availableExercises);
        ExercisePickerSearch = string.Empty;
        IsPickerVisible = true;
    }

    [RelayCommand]
    private void HideExercisePicker() => IsPickerVisible = false;

    partial void OnExercisePickerSearchChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            FilteredPickerExercises = new ObservableCollection<Exercise>(_availableExercises);
        }
        else
        {
            var filtered = _availableExercises
                .Where(e => e.Name.Contains(value, StringComparison.OrdinalIgnoreCase));
            FilteredPickerExercises = new ObservableCollection<Exercise>(filtered);
        }
    }

    [RelayCommand]
    private void AddExercise(Exercise exercise)
    {
        IsPickerVisible = false;
        var existing = ExerciseItems.FirstOrDefault(i => i.Exercise.Id == exercise.Id);
        if (existing is null)
            ExerciseItems.Add(new ActiveExerciseItem { Exercise = exercise });
    }

    // â”€â”€ Set management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [RelayCommand]
    private async Task AddSetAsync(ActiveExerciseItem item)
    {
        if (_currentSession is null) return;

        int setNumber = item.Sets.Count + 1;
        double weight = item.Sets.LastOrDefault()?.Weight ?? 0;
        int reps = item.Sets.LastOrDefault()?.Reps ?? 10;
        string tempo = item.Sets.LastOrDefault()?.Tempo ?? string.Empty;

        var set = await _workoutService.AddSetAsync(
            _currentSession.Id, item.Exercise.Id, setNumber, weight, reps, tempo: tempo);
        item.Sets.Add(set);
    }

    [RelayCommand]
    private async Task CompleteSetAsync(WorkoutSet set)
    {
        set.CompletedAt = DateTime.UtcNow;
        await _workoutService.UpdateSetAsync(set);
    }

    [RelayCommand]
    private async Task DeleteSetAsync((ActiveExerciseItem Item, WorkoutSet Set) args)
    {
        await _workoutService.DeleteSetAsync(args.Set);
        args.Item.Sets.Remove(args.Set);
    }

    // â”€â”€ Template management navigation â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [RelayCommand]
    private async Task GoToTemplatesAsync() =>
        await Shell.Current.GoToAsync("templates");

    // â”€â”€ Elapsed timer â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void StartElapsedTimer()
    {
        var started = DateTime.UtcNow;
        _elapsedTimer = new System.Timers.Timer(1000);
        _elapsedTimer.Elapsed += (_, _) =>
        {
            var elapsed = DateTime.UtcNow - started;
            MainThread.BeginInvokeOnMainThread(() =>
                ElapsedTime = elapsed.ToString(@"mm\:ss"));
        };
        _elapsedTimer.Start();
    }
}

