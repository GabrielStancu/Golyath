using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Golyath.Application.Services;
using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.UI.Controls;
using Golyath.UI.ViewModels.Workout;

namespace Golyath.UI.ViewModels.Exercises;

[QueryProperty(nameof(ExerciseId), "ExerciseId")]
[QueryProperty(nameof(FromPickerParam), "FromPicker")]
public partial class ExerciseDetailViewModel : ObservableObject
{
    private readonly IExerciseRepository _exerciseRepository;
    private readonly IExerciseService _exerciseService;
    private IDispatcherTimer? _imageTimer;

    [ObservableProperty]
    private int _exerciseId;

    [ObservableProperty]
    private string _fromPickerParam = string.Empty;

    [ObservableProperty]
    private Exercise? _exercise;

    [ObservableProperty]
    private int _carouselPosition;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasImages;

    [ObservableProperty]
    private bool _isCustom;

    public ObservableCollection<ImageSource> Images { get; } = [];

    private bool IsFromPicker =>
        string.Equals(FromPickerParam, "true", StringComparison.OrdinalIgnoreCase);

    public ExerciseDetailViewModel(IExerciseRepository exerciseRepository, IExerciseService exerciseService)
    {
        _exerciseRepository = exerciseRepository;
        _exerciseService = exerciseService;
    }

    partial void OnExerciseIdChanged(int value) =>
        _ = LoadAsync(value);

    private async Task LoadAsync(int id)
    {
        IsBusy = true;
        HasImages = false;
        IsCustom = false;
        Images.Clear();
        CarouselPosition = 0;
        try
        {
            Exercise = await _exerciseRepository.GetByIdAsync(id);
            IsCustom = Exercise?.IsCustom == true;
            if (Exercise?.ExternalId is { } externalId)
            {
                foreach (var i in new[] { 0, 1 })
                {
                    var path = $"exercises/{externalId}/{i}.jpg";
                    try
                    {
                        // Read all bytes eagerly — avoids deferred stream / ms-appx path issues
                        using var fileStream = await FileSystem.OpenAppPackageFileAsync(path);
                        using var ms = new MemoryStream();
                        await fileStream.CopyToAsync(ms);
                        var bytes = ms.ToArray();
                        // ImageSource.FromStream creates a StreamImageSource; each call
                        // returns a fresh MemoryStream so MAUI can re-read if needed.
                        Images.Add(ImageSource.FromStream(() => new MemoryStream(bytes)));
                    }
                    catch { /* image not bundled — skip */ }
                }
            }
        }
        finally
        {
            HasImages = Images.Count > 0;
            IsBusy = false;
        }
    }

    public void StartCarousel()
    {
        if (Images.Count <= 1) return;
        _imageTimer = Microsoft.Maui.Controls.Application.Current!.Dispatcher.CreateTimer();
        _imageTimer.Interval = TimeSpan.FromSeconds(2);
        _imageTimer.Tick += OnTimerTick;
        _imageTimer.Start();
    }

    public void StopCarousel()
    {
        if (_imageTimer is null) return;
        _imageTimer.Tick -= OnTimerTick;
        _imageTimer.Stop();
        _imageTimer = null;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (Images.Count == 0) return;
        CarouselPosition = (CarouselPosition + 1) % Images.Count;
    }

    [RelayCommand]
    private async Task DeleteExercise()
    {
        if (Exercise is null || !Exercise.IsCustom) return;

        var usageCount = await _exerciseService.GetWorkoutUsageCountAsync(Exercise.Id);
        var message = usageCount > 0
            ? $"This exercise has been used in {usageCount} workout(s). Deleting it will also remove all related history data. This cannot be undone."
            : "Permanently delete this custom exercise? This cannot be undone.";

        var popup = new ConfirmPopup(
            "Delete Exercise",
            message,
            "Delete",
            "Cancel",
            isDestructive: true);
        var result = await popup.ShowAsync();
        if (result is not true) return;

        await _exerciseService.DeleteCustomExerciseAsync(Exercise.Id);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task AddToWorkout()
    {
        if (Exercise is null) return;
        WeakReferenceMessenger.Default.Send(new ExercisePickedMessage(Exercise));
        await Shell.Current.GoToAsync(IsFromPicker ? "../.." : "..");
    }
}

