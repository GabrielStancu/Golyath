using CommunityToolkit.Mvvm.Input;

namespace Golyath.Controls;

public partial class SetEntryRow : ContentView
{
    public static readonly BindableProperty SetNumberProperty =
        BindableProperty.Create(nameof(SetNumber), typeof(int), typeof(SetEntryRow), 1);

    public static readonly BindableProperty WeightProperty =
        BindableProperty.Create(nameof(Weight), typeof(double), typeof(SetEntryRow), 0.0,
            BindingMode.TwoWay);

    public static readonly BindableProperty RepsProperty =
        BindableProperty.Create(nameof(Reps), typeof(int), typeof(SetEntryRow), 0,
            BindingMode.TwoWay);

    public static readonly BindableProperty IsWarmupProperty =
        BindableProperty.Create(nameof(IsWarmup), typeof(bool), typeof(SetEntryRow), false,
            BindingMode.TwoWay);

    public static readonly BindableProperty IsCompleteProperty =
        BindableProperty.Create(nameof(IsComplete), typeof(bool), typeof(SetEntryRow), false,
            BindingMode.TwoWay, propertyChanged: OnIsCompleteChanged);

    public int SetNumber
    {
        get => (int)GetValue(SetNumberProperty);
        set => SetValue(SetNumberProperty, value);
    }

    public double Weight
    {
        get => (double)GetValue(WeightProperty);
        set => SetValue(WeightProperty, value);
    }

    public int Reps
    {
        get => (int)GetValue(RepsProperty);
        set => SetValue(RepsProperty, value);
    }

    public bool IsWarmup
    {
        get => (bool)GetValue(IsWarmupProperty);
        set => SetValue(IsWarmupProperty, value);
    }

    public bool IsComplete
    {
        get => (bool)GetValue(IsCompleteProperty);
        set => SetValue(IsCompleteProperty, value);
    }

    public event EventHandler? Completed;

    public SetEntryRow()
    {
        InitializeComponent();
    }

    [RelayCommand]
    private void ToggleComplete()
    {
        IsComplete = !IsComplete;
        if (IsComplete)
            Completed?.Invoke(this, EventArgs.Empty);
    }

    private static void OnIsCompleteChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Visual update handled via binding in XAML
    }
}
