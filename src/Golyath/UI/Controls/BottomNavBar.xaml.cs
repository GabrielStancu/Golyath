using Golyath.UI.Views.Workout;
using MauiApp = Microsoft.Maui.Controls.Application;

namespace Golyath.UI.Controls;

public partial class BottomNavBar : ContentView
{
    // ── BindableProperty ──────────────────────────────────────────────────
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(
            nameof(ActiveTab),
            typeof(string),
            typeof(BottomNavBar),
            "home",
            propertyChanged: OnActiveTabChanged);

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    // ── Colors ────────────────────────────────────────────────────────────
    private static readonly Color _activeColor   = Color.FromArgb("#FFD700");
    private static readonly Color _inactiveColor = Color.FromArgb("#888888");

    private readonly NotchedNavBarDrawable _drawable = new();

    // ── Constructor ───────────────────────────────────────────────────────
    public BottomNavBar()
    {
        InitializeComponent();

        SyncDrawableTheme();
        NavBg.Drawable = _drawable;

        if (MauiApp.Current is not null)
            MauiApp.Current.RequestedThemeChanged += OnThemeChanged;

        ApplyTabColors("home");
    }

    // ── Theme sync ────────────────────────────────────────────────────────
    private void SyncDrawableTheme()
    {
        var isDark = MauiApp.Current?.RequestedTheme == AppTheme.Dark;
        _drawable.FillColor = isDark
            ? Color.FromArgb("#1E1E1E")
            : Colors.White;
        NavBg?.Invalidate();
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
        => SyncDrawableTheme();

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (Handler is null && MauiApp.Current is not null)
            MauiApp.Current.RequestedThemeChanged -= OnThemeChanged;
    }

    // ── ActiveTab property-changed callback ───────────────────────────────
    private static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomNavBar bar && newValue is string tab)
            bar.ApplyTabColors(tab);
    }

    private void ApplyTabColors(string activeTab)
    {
        Colorize(HomeIcon,     HomeLabel,     activeTab == "home");
        Colorize(TrainIcon,    TrainLabel,    activeTab == "train");
        Colorize(HistoryIcon,  HistoryLabel,  activeTab == "history");
        Colorize(ProgressIcon, ProgressLabel, activeTab == "progress");
    }

    private static void Colorize(Label icon, Label label, bool active)
    {
        var c = active ? _activeColor : _inactiveColor;
        icon.TextColor  = c;
        label.TextColor = c;
    }

    // ── Tab navigation ────────────────────────────────────────────────────
    private async void OnHomeTabTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//DashboardPage");

    private async void OnTrainTabTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//WorkoutTemplatesPage");

    private async void OnHistoryTabTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//HistoryPage");

    private async void OnProgressTabTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//AnalyticsPage");

    // ── FAB: start a free workout ─────────────────────────────────────────
    private async void OnFabTapped(object sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync(nameof(ActiveWorkoutPage));
}
