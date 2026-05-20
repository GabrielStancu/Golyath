using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.UI.Views.Workout;

namespace Golyath.UI.ViewModels.Workout;

public partial class WorkoutTemplatesViewModel : ObservableObject, IRecipient<RoutineChangedMessage>
{
    private readonly IRoutineService _routineService;
    private readonly IWorkoutService _workoutService;

    public ObservableCollection<RoutineSummaryDto> Routines { get; } = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasRoutines;

    public bool HasNoRoutines => !HasRoutines;

    public WorkoutTemplatesViewModel(IRoutineService routineService, IWorkoutService workoutService)
    {
        _routineService = routineService;
        _workoutService = workoutService;
    }

    public void RegisterMessenger()
    {
        WeakReferenceMessenger.Default.Unregister<RoutineChangedMessage>(this);
        WeakReferenceMessenger.Default.Register(this);
    }

    public void UnregisterMessenger() =>
        WeakReferenceMessenger.Default.Unregister<RoutineChangedMessage>(this);

    public async void Receive(RoutineChangedMessage message) =>
        await LoadAsync();

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var routines = await _routineService.GetAllRoutinesAsync();
            Routines.Clear();
            foreach (var r in routines)
                Routines.Add(r);

            HasRoutines = Routines.Count > 0;
            OnPropertyChanged(nameof(HasNoRoutines));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartFreeWorkout()
    {
        await Shell.Current.GoToAsync(nameof(ActiveWorkoutPage));
    }

    [RelayCommand]
    private async Task StartRoutine(RoutineSummaryDto routine)
    {
        var workout = await _workoutService.StartWorkoutFromRoutineAsync(routine.Id);
        await Shell.Current.GoToAsync($"{nameof(ActiveWorkoutPage)}?workoutId={workout.Id}");
    }

    [RelayCommand]
    private async Task EditRoutine(RoutineSummaryDto routine)
    {
        await Shell.Current.GoToAsync($"{nameof(RoutineBuilderPage)}?routineId={routine.Id}");
    }

    [RelayCommand]
    private async Task DeleteRoutine(RoutineSummaryDto routine)
    {
        bool confirm = await Shell.Current.DisplayAlert("Delete Routine",
            $"Are you sure you want to delete \"{routine.Name}\"?", "Delete", "Cancel");
        if (!confirm) return;

        await _routineService.DeleteRoutineAsync(routine.Id);
        Routines.Remove(routine);
        HasRoutines = Routines.Count > 0;
        OnPropertyChanged(nameof(HasNoRoutines));
    }

    [RelayCommand]
    private async Task NewRoutine()
    {
        await Shell.Current.GoToAsync(nameof(RoutineBuilderPage));
    }
}
