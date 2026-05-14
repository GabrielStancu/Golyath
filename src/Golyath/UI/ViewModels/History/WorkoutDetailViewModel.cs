using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.UI.Controls;
using Golyath.UI.ViewModels.Workout;

namespace Golyath.UI.ViewModels.History;

[QueryProperty(nameof(WorkoutId), "workoutId")]
public partial class WorkoutDetailViewModel : ObservableObject
{
    private readonly IWorkoutHistoryService _historyService;
    private readonly IWorkoutService _workoutService;
    private bool _initializing;

    [ObservableProperty] private int _workoutId;
    [ObservableProperty] private WorkoutHistoryDetailDto? _detail;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _notFound;
    [ObservableProperty] private string _editableNotes = string.Empty;

    public ObservableCollection<TagChipViewModel> Tags { get; } = [];
    public ObservableCollection<HistoryExerciseViewModel> Exercises { get; } = [];

    public WorkoutDetailViewModel(IWorkoutHistoryService historyService, IWorkoutService workoutService)
    {
        _historyService = historyService;
        _workoutService = workoutService;
    }

    partial void OnWorkoutIdChanged(int value)
    {
        if (value > 0)
            _ = LoadAsync();
    }

    partial void OnEditableNotesChanged(string value)
    {
        if (_initializing || WorkoutId <= 0) return;
        var notes = string.IsNullOrWhiteSpace(value) ? null : value;
        _ = _workoutService.UpdateWorkoutNotesAsync(WorkoutId, notes);
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        _initializing = true;
        try
        {
            Detail = await _historyService.GetWorkoutDetailAsync(WorkoutId);
            NotFound = Detail is null;
            if (Detail is not null)
            {
                EditableNotes = Detail.Notes ?? string.Empty;
                await LoadTagsAsync();
                LoadExercises(Detail);
            }
        }
        finally
        {
            _initializing = false;
            IsBusy = false;
        }
    }

    private void LoadExercises(WorkoutHistoryDetailDto detail)
    {
        Exercises.Clear();
        foreach (var ex in detail.Exercises)
            Exercises.Add(new HistoryExerciseViewModel(ex, _workoutService));
    }

    private async Task LoadTagsAsync()
    {
        Tags.Clear();
        var tags = await _historyService.GetTagsForWorkoutAsync(WorkoutId);
        foreach (var tag in tags)
            Tags.Add(new TagChipViewModel(tag, RemoveTagAsync));
    }

    private async Task RemoveTagAsync(TagChipViewModel chip)
    {
        await _historyService.RemoveTagAsync(WorkoutId, chip.TagId);
        Tags.Remove(chip);
    }

    [RelayCommand]
    private async Task AddTag()
    {
        var allTags = await _historyService.GetAllTagsAsync();
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

        var tag = await _historyService.GetOrCreateTagAsync(tagName);
        if (Tags.Any(t => t.TagId == tag.Id)) return;

        await _historyService.AssignTagAsync(WorkoutId, tag.Id);
        Tags.Add(new TagChipViewModel(tag, RemoveTagAsync));
    }

    [RelayCommand]
    private Task GoBackAsync() => Shell.Current.GoToAsync("..");
}
