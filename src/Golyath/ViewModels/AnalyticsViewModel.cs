using CommunityToolkit.Mvvm.ComponentModel;

namespace Golyath.ViewModels;

public partial class AnalyticsViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _placeholderMessage = "Analytics coming in Phase 3";

    public AnalyticsViewModel()
    {
        Title = "Analytics";
    }
}
