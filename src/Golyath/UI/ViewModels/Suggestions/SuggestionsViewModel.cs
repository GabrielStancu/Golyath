using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Application.DTOs;
using Golyath.Application.Services;

namespace Golyath.UI.ViewModels.Suggestions;

public partial class SuggestionsViewModel : ObservableObject
{
    private readonly ISuggestionsService _suggestions;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private IReadOnlyList<TrainingSuggestion> _items = [];
    [ObservableProperty] private bool _hasItems;
    [ObservableProperty] private bool _isEmpty;

    public SuggestionsViewModel(ISuggestionsService suggestions)
    {
        _suggestions = suggestions;
    }

    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var result = await _suggestions.GetSuggestionsAsync();
            Items = result;
            HasItems = result.Count > 0;
            IsEmpty = result.Count == 0;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync();
}
