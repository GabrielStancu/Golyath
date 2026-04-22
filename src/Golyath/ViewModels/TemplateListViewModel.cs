using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Golyath.Models;
using Golyath.Services;
using System.Collections.ObjectModel;

namespace Golyath.ViewModels;

public partial class TemplateListViewModel : BaseViewModel
{
    private readonly ITemplateService _templateService;

    [ObservableProperty] private ObservableCollection<WorkoutTemplate> _templates = [];
    [ObservableProperty] private bool _isEmpty;

    public TemplateListViewModel(ITemplateService templateService)
    {
        _templateService = templateService;
        Title = "My Templates";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var list = await _templateService.GetTemplatesAsync();
            Templates = new ObservableCollection<WorkoutTemplate>(
                list.OrderByDescending(t => t.LastUsedAt ?? t.CreatedAt));
            IsEmpty = Templates.Count == 0;
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task CreateTemplateAsync() =>
        await Shell.Current.GoToAsync("template-edit");

    [RelayCommand]
    private async Task EditTemplateAsync(WorkoutTemplate template) =>
        await Shell.Current.GoToAsync($"template-edit?templateId={template.Id}");

    [RelayCommand]
    private async Task DeleteTemplateAsync(WorkoutTemplate template)
    {
        bool confirm = await Shell.Current.DisplayAlert(
            "Delete Template", $"Delete \"{template.Name}\"?", "Delete", "Cancel");
        if (!confirm) return;

        await _templateService.DeleteTemplateAsync(template.Id);
        Templates.Remove(template);
        IsEmpty = Templates.Count == 0;
    }

    [RelayCommand]
    private async Task GoBackAsync() =>
        await Shell.Current.GoToAsync("..");
}
