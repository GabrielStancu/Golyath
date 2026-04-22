using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Models;
using Golyath.Services;
using System.Collections.ObjectModel;

namespace Golyath.ViewModels;

/// <summary>One exercise entry inside the template being edited.</summary>
public partial class TemplateEntryItem : ObservableObject
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;

    [ObservableProperty] private int _targetSets = 3;
    [ObservableProperty] private int _targetReps = 10;
    [ObservableProperty] private double _targetWeight;
    [ObservableProperty] private string _targetTempo = string.Empty;

    public WorkoutTemplateEntry ToEntry(int templateId, int sortOrder) => new()
    {
        TemplateId = templateId,
        ExerciseId = ExerciseId,
        SortOrder = sortOrder,
        TargetSets = TargetSets,
        TargetReps = TargetReps,
        TargetWeight = TargetWeight,
        TargetTempo = TargetTempo,
        RestSeconds = 90,
    };
}

public partial class TemplateEditViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ITemplateService _templateService;
    private readonly IExerciseService _exerciseService;

    private int _templateId;

    // ── Template fields ──────────────────────────────────────────────
    [ObservableProperty] private string _templateName = string.Empty;
    [ObservableProperty] private string _templateDescription = string.Empty;

    // ── Entries ──────────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<TemplateEntryItem> _entries = [];

    // ── Exercise picker ──────────────────────────────────────────────
    [ObservableProperty] private bool _isPickerVisible;
    [ObservableProperty] private string _pickerSearch = string.Empty;
    [ObservableProperty] private ObservableCollection<Exercise> _filteredExercises = [];
    private ObservableCollection<Exercise> _allExercises = [];

    // ── State ────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    private bool _isNewTemplate = true;
    public bool IsEditing => !IsNewTemplate;

    public TemplateEditViewModel(ITemplateService templateService, IExerciseService exerciseService)
    {
        _templateService = templateService;
        _exerciseService = exerciseService;
        Title = "New Template";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("templateId", out object? val) &&
            int.TryParse(val?.ToString(), out int id) && id > 0)
        {
            _templateId = id;
            IsNewTemplate = false;
            Title = "Edit Template";
            _ = LoadAsync(id);
        }
    }

    private async Task LoadAsync(int id)
    {
        var template = await _templateService.GetTemplateByIdAsync(id);
        if (template is null) return;

        TemplateName = template.Name;
        TemplateDescription = template.Description;

        var details = await _templateService.GetEntryDetailsAsync(id);
        Entries = new ObservableCollection<TemplateEntryItem>(details.Select(d => new TemplateEntryItem
        {
            ExerciseId = d.Entry.ExerciseId,
            ExerciseName = d.Exercise.Name,
            TargetSets = d.Entry.TargetSets,
            TargetReps = d.Entry.TargetReps,
            TargetWeight = d.Entry.TargetWeight,
            TargetTempo = d.Entry.TargetTempo,
        }));
    }

    // ── Save ─────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(TemplateName))
        {
            await Shell.Current.DisplayAlert("Validation", "Template name is required.", "OK");
            return;
        }

        if (IsNewTemplate)
        {
            var template = await _templateService.CreateTemplateAsync(new WorkoutTemplate
            {
                Name = TemplateName.Trim(),
                Description = TemplateDescription.Trim(),
            });
            _templateId = template.Id;
            IsNewTemplate = false;
        }
        else
        {
            var template = await _templateService.GetTemplateByIdAsync(_templateId);
            if (template is not null)
            {
                template.Name = TemplateName.Trim();
                template.Description = TemplateDescription.Trim();
                await _templateService.UpdateTemplateAsync(template);
            }
        }

        var entries = Entries.Select((e, i) => e.ToEntry(_templateId, i));
        await _templateService.SaveEntriesAsync(_templateId, entries);

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task DeleteTemplateAsync()
    {
        if (_templateId == 0) { await Shell.Current.GoToAsync(".."); return; }

        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Template", $"Delete \"{TemplateName}\"?", "Delete", "Cancel");
        if (!confirm) return;

        await _templateService.DeleteTemplateAsync(_templateId);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private void DeleteEntry(TemplateEntryItem entry) =>
        Entries.Remove(entry);

    [RelayCommand]
    private async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");

    // ── Exercise Picker ──────────────────────────────────────────────

    [RelayCommand]
    private async Task ShowPickerAsync()
    {
        if (_allExercises.Count == 0)
        {
            var all = await _exerciseService.GetAllExercisesAsync();
            _allExercises = new ObservableCollection<Exercise>(all.OrderBy(e => e.Name));
        }
        FilteredExercises = new ObservableCollection<Exercise>(_allExercises);
        PickerSearch = string.Empty;
        IsPickerVisible = true;
    }

    [RelayCommand]
    private void HidePicker() => IsPickerVisible = false;

    partial void OnPickerSearchChanged(string value)
    {
        FilteredExercises = string.IsNullOrWhiteSpace(value)
            ? new ObservableCollection<Exercise>(_allExercises)
            : new ObservableCollection<Exercise>(
                _allExercises.Where(e => e.Name.Contains(value, StringComparison.OrdinalIgnoreCase)));
    }

    [RelayCommand]
    private void AddExercise(Exercise exercise)
    {
        IsPickerVisible = false;
        if (Entries.Any(e => e.ExerciseId == exercise.Id)) return;
        Entries.Add(new TemplateEntryItem
        {
            ExerciseId = exercise.Id,
            ExerciseName = exercise.Name,
        });
    }
}
