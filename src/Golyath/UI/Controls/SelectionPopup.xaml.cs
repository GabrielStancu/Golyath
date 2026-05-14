namespace Golyath.UI.Controls;

public class SelectionPopup : Border
{
    private readonly string? _currentValue;

    public SelectionPopup(string title, IReadOnlyList<string> options, string? currentValue = null)
    {
        _currentValue = currentValue;

        var app = Microsoft.Maui.Controls.Application.Current!;
        var isDark = app.RequestedTheme == AppTheme.Dark;
        var accentColor = (Color)app.Resources["Accent"];

        // Card styling
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 };
        Stroke = isDark ? Color.FromArgb("#333333") : Color.FromArgb("#E0E0E0");
        StrokeThickness = 1;
        BackgroundColor = isDark ? Color.FromArgb("#1A1A1A") : Color.FromArgb("#FFFFFF");
        Padding = 0;
        WidthRequest = 320;
        MaximumHeightRequest = 400;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;

        // Title bar
        var titleLabel = new Label
        {
            Text = title,
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = isDark ? Colors.White : Color.FromArgb("#111111"),
            VerticalOptions = LayoutOptions.Center,
        };

        var closeIcon = new Label
        {
            Text = "\uE5CD",
            FontFamily = "MaterialIcons",
            FontSize = 22,
            TextColor = isDark ? Color.FromArgb("#666666") : Color.FromArgb("#888888"),
            VerticalOptions = LayoutOptions.Center,
        };
        var closeTap = new TapGestureRecognizer();
        closeTap.Tapped += (_, _) => ModalOverlay.Dismiss(this, null);
        closeIcon.GestureRecognizers.Add(closeTap);

        var titleGrid = new Grid
        {
            ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)],
            Padding = new Thickness(20, 18, 16, 14),
        };
        titleGrid.Add(titleLabel, 0);
        titleGrid.Add(closeIcon, 1);

        // Divider
        var divider = new BoxView
        {
            HeightRequest = 1,
            Color = isDark ? Color.FromArgb("#2E2E2E") : Color.FromArgb("#E0E0E0"),
        };

        // Options
        var optionsLayout = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(8, 6, 8, 8) };
        foreach (var option in options)
        {
            var isSelected = string.Equals(option, _currentValue, StringComparison.OrdinalIgnoreCase);

            var label = new Label
            {
                Text = option,
                FontSize = 15,
                Padding = new Thickness(14, 13),
                TextColor = isSelected ? accentColor : (isDark ? Colors.White : Color.FromArgb("#111111")),
                FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None,
            };

            var optionBorder = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                Stroke = Colors.Transparent,
                BackgroundColor = isSelected
                    ? (isDark ? Color.FromArgb("#2A2410") : Color.FromArgb("#FFF8E1"))
                    : Colors.Transparent,
                Padding = 0,
                Margin = new Thickness(0, 1),
                Content = label,
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => ModalOverlay.Dismiss(this, option);
            optionBorder.GestureRecognizers.Add(tap);

            optionsLayout.Children.Add(optionBorder);
        }

        var scrollView = new ScrollView { MaximumHeightRequest = 300, Content = optionsLayout };

        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Children.Add(titleGrid);
        stack.Children.Add(divider);
        stack.Children.Add(scrollView);

        Content = stack;
    }

    public Task<object?> ShowAsync(Page? page = null) => ModalOverlay.ShowAsync(this, page);
}

