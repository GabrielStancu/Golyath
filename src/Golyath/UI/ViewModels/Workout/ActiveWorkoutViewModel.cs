using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Golyath.Application.Services;
using Golyath.Core.Abstractions;
using Golyath.UI.Controls;
using WorkoutEntity = Golyath.Core.Entities.Workout;
using Golyath.Core.Entities;

namespace Golyath.UI.ViewModels.Workout;

public partial class ActiveWorkoutViewModel : ObservableObject, IRecipient<ExercisePickedMessage>
{
    private readonly IWorkoutService _workoutService;
    private readonly IExerciseRepository _exerciseRepository;
    private readonly ITagService _tagService;
    private readonly ISettingsService _settingsService;
    private WorkoutEntity? _workout;
    private IDispatcherTimer? _workoutTimer;
    private IDispatcherTimer? _restTimer;
    private int _elapsedSeconds;

    public ObservableCollection<WorkoutExerciseViewModel> Exercises { get; } = [];
    public ObservableCollection<TagChipViewModel> WorkoutTags { get; } = [];

    [ObservableProperty]
    private string _workoutTitle = "New Workout";

    [ObservableProperty]
    private string _elapsedTime = "00:00";

    [ObservableProperty]
    private bool _isRestTimerVisible;

    [ObservableProperty]
    private string _restTimeDisplay = "01:30";

    [ObservableProperty]
    private int _restSecondsRemaining;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _workoutNotes = string.Empty;

    public event EventHandler? WorkoutCompleted;
    public event EventHandler? AddExerciseRequested;

    public ActiveWorkoutViewModel(IWorkoutService workoutService, IExerciseRepository exerciseRepository, ITagService tagService, ISettingsService settingsService)
    {
        _workoutService = workoutService;
        _exerciseRepository = exerciseRepository;
        _tagService = tagService;
        _settingsService = settingsService;
    }

    partial void OnWorkoutNotesChanged(string value)
    {
        if (_workout is null) return;
        var notes = string.IsNullOrWhiteSpace(value) ? null : value;
        _ = _workoutService.UpdateWorkoutNotesAsync(_workout.Id, notes);
    }

    public void RegisterMessenger()
    {
        WeakReferenceMessenger.Default.Unregister<ExercisePickedMessage>(this);
        WeakReferenceMessenger.Default.Register(this);
    }

    public void UnregisterMessenger() =>
        WeakReferenceMessenger.Default.Unregister<ExercisePickedMessage>(this);

    public async void Receive(ExercisePickedMessage message) =>
        await AddExerciseAsync(message.Value);

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var active = await _workoutService.GetActiveWorkoutAsync();
            if (active is not null)
            {
                _workout = active;
                WorkoutTitle = active.Name ?? "Workout";
                WorkoutNotes = active.Notes ?? string.Empty;
                _elapsedSeconds = (int)(DateTime.UtcNow - active.StartedAt).TotalSeconds;
                await LoadExercisesAsync();
                await LoadTagsAsync();
            }
            else
            {
                _workout = await _workoutService.StartWorkoutAsync();
                WorkoutTitle = "New Workout";
                WorkoutNotes = string.Empty;
                _elapsedSeconds = 0;
            }
            StartWorkoutTimer();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadExercisesAsync()
    {
        if (_workout is null) return;

        var workoutExercises = await _workoutService.GetWorkoutExercisesAsync(_workout.Id);
        Exercises.Clear();

        foreach (var we in workoutExercises)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(we.ExerciseId);
            if (exercise is null) continue;

            var exerciseVm = CreateExerciseViewModel(we, exercise);
            var sets = await _workoutService.GetSetsForExerciseAsync(we.Id);
            exerciseVm.LoadSets(sets);
            Exercises.Add(exerciseVm);
        }
    }

    private WorkoutExerciseViewModel CreateExerciseViewModel(WorkoutExercise we, Exercise exercise)
    {
        var vm = new WorkoutExerciseViewModel(we, exercise, _workoutService);
        vm.SetCompleted += OnSetCompleted;
        vm.RemoveRequested += OnRemoveExercise;
        return vm;
    }

    public async Task AddExerciseAsync(Exercise exercise)
    {
        if (_workout is null) return;

        var lastSet = await _workoutService.GetLastSetForAutofillAsync(exercise.Id);
        double weight = lastSet?.Weight ?? 0;
        int reps = lastSet?.Reps ?? 10;

        var we = await _workoutService.AddExerciseAsync(_workout.Id, exercise.Id);
        var exerciseVm = CreateExerciseViewModel(we, exercise);

        var firstSet = await _workoutService.AddSetAsync(we.Id, weight, reps);
        exerciseVm.LoadSets([firstSet]);

        Exercises.Add(exerciseVm);
    }

    [RelayCommand]
    private void RequestAddExercise() => AddExerciseRequested?.Invoke(this, EventArgs.Empty);

    // ─── Tags ─────────────────────────────────────────────────────────────────

    private async Task LoadTagsAsync()
    {
        if (_workout is null) return;
        WorkoutTags.Clear();
        var tags = await _tagService.GetTagsForWorkoutAsync(_workout.Id);
        foreach (var tag in tags)
            WorkoutTags.Add(new TagChipViewModel(tag, RemoveTagAsync));
    }

    [RelayCommand]
    private async Task AddTag()
    {
        if (_workout is null) return;

        var allTags = await _tagService.GetAllTagsAsync();
        var options = allTags.Select(t => t.Name).Append("+ Create new tag").ToList();

        var popup = new SelectionPopup("Add Tag", options);
        var choice = await popup.ShowAsync();
        if (choice is not string selected || string.IsNullOrEmpty(selected)) return;

        string tagName;
        if (selected == "+ Create new tag")
        {
            var inputPopup = new InputPopup("New Tag", "Enter tag name:", maxLength: 50);
            var inputResult = await inputPopup.ShowAsync();
            tagName = inputResult as string ?? string.Empty;
            if (string.IsNullOrWhiteSpace(tagName)) return;
        }
        else
        {
            tagName = selected;
        }

        // Skip if already attached
        if (WorkoutTags.Any(c => c.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase)))
            return;

        var tag = await _tagService.GetOrCreateTagAsync(tagName);
        await _tagService.AddTagToWorkoutAsync(_workout.Id, tag.Id);
        WorkoutTags.Add(new TagChipViewModel(tag, RemoveTagAsync));
    }

    private async Task RemoveTagAsync(TagChipViewModel chip)
    {
        if (_workout is null) return;
        await _tagService.RemoveTagFromWorkoutAsync(_workout.Id, chip.TagId);
        WorkoutTags.Remove(chip);
    }

    [RelayCommand]
    private async Task CompleteWorkout()
    {
        if (_workout is null) return;

        var popup = new ConfirmPopup("Finish Workout", "Save and finish this workout?", "Finish", "Cancel");
        var result = await popup.ShowAsync();
        if (result is not true) return;

        StopTimers();
        await _workoutService.CompleteWorkoutAsync(_workout.Id);
        WeakReferenceMessenger.Default.Send(new WorkoutChangedMessage(_workout.Id));
        _workout = null;
        WorkoutCompleted?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task AbandonWorkout()
    {
        if (_workout is null) return;

        var popup = new ConfirmPopup("Discard Workout", "Discard this workout? All logged data will be lost.", "Discard", "Cancel", isDestructive: true);
        var result = await popup.ShowAsync();
        if (result is not true) return;

        StopTimers();
        await _workoutService.AbandonWorkoutAsync(_workout.Id);
        WeakReferenceMessenger.Default.Send(new WorkoutChangedMessage(_workout.Id));
        _workout = null;
        WorkoutCompleted?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void SkipRest()
    {
        StopRestTimer();
        IsRestTimerVisible = false;
    }

    private void OnSetCompleted(object? sender, WorkoutSetViewModel _) =>
        StartRestTimer(_settingsService.GetDefaultRestSeconds());

    private async void OnRemoveExercise(object? sender, WorkoutExerciseViewModel vm)
    {
        var popup = new ConfirmPopup(
            "Remove Exercise",
            $"Remove {vm.ExerciseName} and all its sets from this workout?",
            "Remove",
            "Cancel",
            isDestructive: true);
        var result = await popup.ShowAsync();
        if (result is not true) return;

        await _workoutService.RemoveExerciseAsync(vm.WorkoutExerciseId);
        Exercises.Remove(vm);
    }

    // ─── Workout elapsed timer ────────────────────────────────────────────────

    private void StartWorkoutTimer()
    {
        _workoutTimer = Microsoft.Maui.Controls.Application.Current!.Dispatcher.CreateTimer();
        _workoutTimer.Interval = TimeSpan.FromSeconds(1);
        _workoutTimer.Tick += (_, _) =>
        {
            _elapsedSeconds++;
            var ts = TimeSpan.FromSeconds(_elapsedSeconds);
            ElapsedTime = ts.Hours > 0
                ? ts.ToString(@"h\:mm\:ss")
                : ts.ToString(@"mm\:ss");
        };
        _workoutTimer.Start();
    }

    // ─── Rest timer ───────────────────────────────────────────────────────────

    private void StartRestTimer(int seconds)
    {
        StopRestTimer();
        RestSecondsRemaining = seconds;
        UpdateRestDisplay();
        IsRestTimerVisible = true;

        _restTimer = Microsoft.Maui.Controls.Application.Current!.Dispatcher.CreateTimer();
        _restTimer.Interval = TimeSpan.FromSeconds(1);
        _restTimer.Tick += (_, _) =>
        {
            RestSecondsRemaining--;
            UpdateRestDisplay();
            if (RestSecondsRemaining <= 0)
            {
                StopRestTimer();
                IsRestTimerVisible = false;
            }
        };
        _restTimer.Start();
    }

    private void StopRestTimer()
    {
        _restTimer?.Stop();
        _restTimer = null;
    }

    private void StopTimers()
    {
        _workoutTimer?.Stop();
        StopRestTimer();
    }

    private void UpdateRestDisplay()
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, RestSecondsRemaining));
        RestTimeDisplay = ts.ToString(@"mm\:ss");
    }
}
