using Golyath.Core.Enums;

namespace Golyath.UI.Controls;

/// <summary>
/// A record representing a selectable language with its flag emoji and display name.
/// </summary>
internal record LanguageOption(AppLanguage Language, string Flag, string Name);

/// <summary>
/// Modal popup for selecting the app language, showing a flag and name for each option.
/// Returns an <see cref="AppLanguage"/> value via <see cref="ModalOverlay"/>.
/// </summary>
public class LanguageSelectionPopup : Border
{
    /// <summary>All supported language options.</summary>
    internal static readonly IReadOnlyList<LanguageOption> Options =
    [
        new(AppLanguage.English,  "🇬🇧", "English"),
        new(AppLanguage.Romanian, "🇷🇴", "Română"),
    ];

    /// <summary>Returns the flag emoji for the given language.</summary>
    public static string FlagFor(AppLanguage lang) => lang switch
    {
        AppLanguage.Romanian => "🇷🇴",
        _                    => "🇬🇧",
    };

    /// <summary>Returns the display name for the given language.</summary>
    public static string NameFor(AppLanguage lang) => lang switch
    {
        AppLanguage.Romanian => "Română",
        _                    => "English",
    };

    public LanguageSelectionPopup(AppLanguage currentLanguage)
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
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions   = LayoutOptions.Center;

        // ── Title row ────────────────────────────────────────────────────────
        var titleLabel = new Label
        {
            Text             = "LANGUAGE",
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
            ColumnDefinitions =
            [
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            ],
            Padding = new Thickness(16, 14, 12, 12),
        };
        titleGrid.Add(titleLabel, 0);
        titleGrid.Add(closeIcon,  1);

        // ── Divider ──────────────────────────────────────────────────────────
        var divider = new BoxView { HeightRequest = 1, Color = border };

        // ── Option rows ──────────────────────────────────────────────────────
        var optionsLayout = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(8, 6, 8, 8) };

        foreach (var option in Options)
        {
            var isSelected = option.Language == currentLanguage;

            var row = new Grid
            {
                ColumnDefinitions =
                [
                    new ColumnDefinition(GridLength.Auto), // flag
                    new ColumnDefinition(GridLength.Star), // name
                    new ColumnDefinition(GridLength.Auto), // check
                ],
                ColumnSpacing = 10,
                Padding       = new Thickness(12, 11),
            };

            var flagLabel = new Label
            {
                Text            = option.Flag,
                FontSize        = 22,
                WidthRequest    = 32,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
            };

            var nameLabel = new Label
            {
                Text            = option.Name,
                FontSize        = 15,
                TextColor       = isSelected ? accent : textPrim,
                FontAttributes  = isSelected ? FontAttributes.Bold : FontAttributes.None,
                VerticalOptions = LayoutOptions.Center,
            };

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

            row.Add(flagLabel, 0);
            row.Add(nameLabel, 1);
            row.Add(check,     2);

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
            var capturedLanguage = option.Language;
            tap.Tapped += (_, _) => ModalOverlay.Dismiss(this, capturedLanguage);
            optionBorder.GestureRecognizers.Add(tap);

            optionsLayout.Children.Add(optionBorder);
        }

        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Children.Add(titleGrid);
        stack.Children.Add(divider);
        stack.Children.Add(optionsLayout);

        Content = stack;
    }

    public Task<object?> ShowAsync(Page? page = null) => ModalOverlay.ShowAsync(this, page);
}
