using CommunityToolkit.Mvvm.Input;
using Golyath.Core.Entities;

namespace Golyath.UI.ViewModels.Workout;

/// <summary>Wraps a <see cref="Tag"/> for display as a chip in the active workout view.</summary>
public sealed class TagChipViewModel
{
    public int TagId { get; }
    public string Name { get; }
    public IAsyncRelayCommand RemoveCommand { get; }

    public TagChipViewModel(Tag tag, Func<TagChipViewModel, Task> onRemove)
    {
        TagId = tag.Id;
        Name = tag.Name;
        RemoveCommand = new AsyncRelayCommand(() => onRemove(this));
    }
}
