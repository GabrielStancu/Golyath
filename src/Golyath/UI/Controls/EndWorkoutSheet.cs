using Microsoft.Maui.Controls.Shapes;
using Golyath.UI.ViewModels.Workout;

namespace Golyath.UI.Controls;

/// <summary>
/// End-workout popup styled to match the app's card system.
/// Shows a notes field, FINISH &amp; SAVE button, and a Discard option.
/// Returns an <see cref="EndWorkoutResult"/> via ModalOverlay.
/// </summary>
public class EndWorkoutSheet : Border
{
    private readonly Entry _notesEntry;

    public EndWorkoutSheet(
        string currentNotes,
        System.Collections.ObjectModel.ObservableCollection<TagChipViewModel>? workoutTags = null,
        Func<Task>? addTagCallback = null)
    {
        var app      = Microsoft.Maui.Controls.Application.Current!;
        var isDark   = app.RequestedTheme == AppTheme.Dark;
        var accent   = (Color)app.Resources["Accent"];
        var surface  = (Color)app.Resources[isDark ? "CardSurfaceDark"  : "CardSurfaceLight"];
        var bColor   = (Color)app.Resources[isDark ? "CardBorderDark"   : "CardBorderLight"];
        var textPrim = (Color)app.Resources[isDark ? "TextPrimaryDark"  : "TextPrimaryLight"];
        var textMut  = (Color)app.Resources[isDark ? "TextMutedDark"    : "TextMutedLight"];

        // ── Shell
        StrokeShape      = new RoundRectangle { CornerRadius = 4 };
        Stroke           = bColor;
        StrokeThickness  = 1;
        BackgroundColor  = surface;
        Padding          = 0;
        WidthRequest     = 320;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions   = LayoutOptions.Center;

        // ── Title row
        var titleLabel = new Label
        {
            Text = "END WORKOUT",
            FontSize = 10,
            CharacterSpacing = 1.2,
            FontAttributes = FontAttributes.Bold,
            TextColor = textMut,
            VerticalOptions = LayoutOptions.Center,
        };
        var closeLabel = new Label
        {
            Text = "\uE5CD",
            FontFamily = "MaterialIcons",
            FontSize = 20,
            TextColor = textMut,
            VerticalOptions = LayoutOptions.Center,
        };
        closeLabel.GestureRecognizers.Add(TapDismiss(null));

        var titleRow = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            ],
            Padding = new Thickness(16, 14, 12, 12),
        };
        titleRow.Add(titleLabel, 0);
        titleRow.Add(closeLabel, 1);

        var div1 = Divider(bColor);

        // ── Notes entry
        _notesEntry = new Entry
        {
            Placeholder = "Session notes (optional)…",
            PlaceholderColor = Color.FromArgb("#555555"),
            TextColor = textPrim,
            Text = currentNotes,
            BackgroundColor = Colors.Transparent,
            FontSize = 14,
            Margin = new Thickness(16, 14, 16, 14),
        };

        var div2 = Divider(bColor);

        // ── FINISH & SAVE button
        var finishInner = new Label
        {
            Text = "FINISH & SAVE",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            CharacterSpacing = 0.8,
            TextColor = Color.FromArgb("#111111"),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        var finishBorder = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 4 },
            StrokeThickness = 0,
            BackgroundColor = accent,
            Padding = new Thickness(0, 14),
            Margin = new Thickness(16, 14, 16, 8),
            Content = finishInner,
        };
        finishBorder.GestureRecognizers.Add(TapDismiss(
            new EndWorkoutResult("finish", _notesEntry.Text ?? string.Empty),
            useLiveNotes: true));

        // ── Discard link
        var discardLabel = new Label
        {
            Text = "Discard workout",
            FontSize = 13,
            TextColor = Color.FromArgb("#EF5350"),
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 4, 0, 16),
        };
        discardLabel.GestureRecognizers.Add(TapDismiss(
            new EndWorkoutResult("discard", string.Empty)));

        // ── Assemble
        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(titleRow);
        stack.Add(div1);
        stack.Add(_notesEntry);
        stack.Add(div2);
        stack.Add(finishBorder);
        stack.Add(discardLabel);

        Content = stack;
    }

    public Task<object?> ShowAsync(Page? page = null) =>
        ModalOverlay.ShowAsync(this, page);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private TapGestureRecognizer TapDismiss(object? result, bool useLiveNotes = false)
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            var effectiveResult = useLiveNotes && result is EndWorkoutResult ewr
                ? new EndWorkoutResult(ewr.Action, _notesEntry.Text ?? string.Empty)
                : result;
            ModalOverlay.Dismiss(this, effectiveResult);
        };
        return tap;
    }

    private static BoxView Divider(Color color) =>
        new() { HeightRequest = 1, Color = color };
}
