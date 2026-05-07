using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.DTOs;
using Golyath.Application.Services;

namespace Golyath.UI.ViewModels.History;

[QueryProperty(nameof(WorkoutId), "workoutId")]
public partial class WorkoutDetailViewModel : ObservableObject
{
    private readonly IWorkoutHistoryService _historyService;

    [ObservableProperty] private int _workoutId;
    [ObservableProperty] private WorkoutHistoryDetailDto? _detail;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _notFound;

    public WorkoutDetailViewModel(IWorkoutHistoryService historyService)
    {
        _historyService = historyService;
    }

    partial void OnWorkoutIdChanged(int value)
    {
        if (value > 0)
            _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Detail = await _historyService.GetWorkoutDetailAsync(WorkoutId);
            NotFound = Detail is null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task GoBackAsync() => Shell.Current.GoToAsync("..");
}
