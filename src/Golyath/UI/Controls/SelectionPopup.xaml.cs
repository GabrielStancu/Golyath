namespace Golyath.UI.Controls;

public class SelectionPopup : Border
{
    public SelectionPopup(string title, IReadOnlyList<string> options, string? currentValue = null)
    {
        var app    = Microsoft.Maui.Controls.Application.Current!;
        var isDark = app.RequestedTheme == AppTheme.Dark;

        var accent    = (Color)app.Resources["Accent"];
        var surface   = (Color)app.Resources[isDark ? "CardSurfaceDark"  : "CardSurfaceLight"];
        var border    = (Color)app.Resources[isDark ? "CardBorderDark"   : "CardBorderLight"];
        var textPrim  = (Color)app.Resources[isDark ? "TextPrimaryDark"  : "TextPrimaryLight"];
        var textMuted = (Color)app.Resources[isDark ? "TextMutedDark"    : "TextMutedLight"];

        // ── Card shell ───────────────────────────────────────────────────────
        StrokeShape      = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 };
        Stroke           = border;
        StrokeThickness  = 1;
        BackgroundColor  = surface;
        Padding          = 0;
        WidthRequest     = 320;
        MaximumHeightRequest = 480;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions   = LayoutOptions.Center;

        // ── Title row ────────────────────────────────────────────────────────
        var titleLabel = new Label
        {
            Text             = title.ToUpperInvariant(),
            FontSize         = 10,
            CharacterSpacing = 1.2,
            FontAttributes   = FontAttributes.Bold,
            TextColor        = textMuted,
            VerticalOptions  = LayoutOptions.Center,
        };

        var closeIcon = new Label
        {
            Text            = "\uE5CD",
            FontFamily      = "MaterialIcons",
            FontSize        = 20,
            TextColor       = textMuted,
            VerticalOptions = LayoutOptions.Center,
        };
        var closeTap = new TapGestureRecognizer();
        closeTap.Tapped += (_, _) => ModalOverlay.Dismiss(this, null);
        closeIcon.GestureRecognizers.Add(closeTap);

        var titleGrid = new Grid
        {
            ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)],
            Padding = new Thickness(16, 14, 12, 12),
        };
        titleGrid.Add(titleLabel, 0);
        titleGrid.Add(closeIcon, 1);

        // ── Divider ──────────────────────────────────────────────────────────
        var divider = new BoxView { HeightRequest = 1, Color = border };

        // ── Option rows ──────────────────────────────────────────────────────
        var optionsLayout = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(8, 6, 8, 8) };

        foreach (var option in options)
        {
            var isSelected = string.Equals(option, currentValue, StringComparison.OrdinalIgnoreCase);

            var row = new Grid
            {
                ColumnDefinitions = [new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star)],
                ColumnSpacing = 10,
                Padding = new Thickness(12, 11),
            };

            // Check glyph — visible only for selected item
            var check = new Label
            {
                Text            = "\uE876",
                FontFamily      = "MaterialIcons",
                FontSize        = 16,
                TextColor       = accent,
                WidthRequest    = 20,
                IsVisible       = isSelected,
                VerticalOptions = LayoutOptions.Center,
            };

            var label = new Label
            {
                Text            = option,
                FontSize        = 15,
                TextColor       = isSelected ? accent : textPrim,
                FontAttributes  = isSelected ? FontAttributes.Bold : FontAttributes.None,
                VerticalOptions = LayoutOptions.Center,
            };

            row.Add(check, 0);
            row.Add(label, 1);

            var optionBorder = new Border
            {
                StrokeShape     = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 },
                Stroke          = Colors.Transparent,
                BackgroundColor = isSelected ? accent.WithAlpha(0.12f) : Colors.Transparent,
                Padding         = 0,
                Margin          = new Thickness(0, 1),
                Content         = row,
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => ModalOverlay.Dismiss(this, option);
            optionBorder.GestureRecognizers.Add(tap);

            optionsLayout.Children.Add(optionBorder);
        }

        var scrollView = new ScrollView { MaximumHeightRequest = 360, Content = optionsLayout };

        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Children.Add(titleGrid);
        stack.Children.Add(divider);
        stack.Children.Add(scrollView);

        Content = stack;
    }

    public Task<object?> ShowAsync(Page? page = null) => ModalOverlay.ShowAsync(this, page);
}

