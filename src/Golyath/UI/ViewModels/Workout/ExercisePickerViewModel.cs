using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Golyath.Core.Abstractions;
using Golyath.Core.Entities;

namespace Golyath.UI.ViewModels.Workout;

public partial class ExercisePickerViewModel : ObservableObject
{
    private readonly IExerciseRepository _exerciseRepository;
    private IReadOnlyList<Exercise> _allExercises = [];

    public ObservableCollection<Exercise> Exercises { get; } = [];

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    public ExercisePickerViewModel(IExerciseRepository exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            _allExercises = await _exerciseRepository.GetAllAsync();
            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var query = SearchQuery.Trim().ToLowerInvariant();
        var filtered = string.IsNullOrEmpty(query)
            ? _allExercises
            : _allExercises.Where(e => e.Name.ToLowerInvariant().Contains(query));

        Exercises.Clear();
        foreach (var e in filtered.OrderBy(e => e.Name))
            Exercises.Add(e);
    }

    [RelayCommand]
    private async Task SelectExercise(Exercise exercise)
    {
        WeakReferenceMessenger.Default.Send(new ExercisePickedMessage(exercise));
        await Shell.Current.GoToAsync("..");
    }
}
