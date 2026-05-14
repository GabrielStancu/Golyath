namespace Golyath.UI.Controls;

public class InputPopup : Border
{
    private readonly Entry _entry;

    public InputPopup(string title, string message, string placeholder = "", int maxLength = 100)
    {
        var app = Microsoft.Maui.Controls.Application.Current!;
        var isDark = app.RequestedTheme == AppTheme.Dark;
        var accentColor = (Color)app.Resources["Accent"];
        var accentText = (Color)app.Resources["AccentText"];

        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 20 };
        Stroke = isDark ? Color.FromArgb("#333333") : Color.FromArgb("#E0E0E0");
        StrokeThickness = 1;
        BackgroundColor = isDark ? Color.FromArgb("#1A1A1A") : Color.FromArgb("#FFFFFF");
        Padding = new Thickness(24, 22, 24, 20);
        WidthRequest = 320;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;

        var titleLabel = new Label
        {
            Text = title,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = isDark ? Colors.White : Color.FromArgb("#111111"),
        };

        var messageLabel = new Label
        {
            Text = message,
            FontSize = 14,
            TextColor = isDark ? Color.FromArgb("#AAAAAA") : Color.FromArgb("#666666"),
        };

        _entry = new Entry
        {
            Placeholder = placeholder,
            MaxLength = maxLength,
            FontSize = 15,
            TextColor = isDark ? Colors.White : Color.FromArgb("#111111"),
            PlaceholderColor = isDark ? Color.FromArgb("#666666") : Color.FromArgb("#999999"),
            BackgroundColor = isDark ? Color.FromArgb("#252525") : Color.FromArgb("#F5F5F5"),
            Margin = new Thickness(0, 8, 0, 0),
        };
        _entry.Completed += (_, _) => ModalOverlay.Dismiss(this, _entry.Text?.Trim());

        var cancelButton = new Button
        {
            Text = "Cancel",
            BackgroundColor = isDark ? Color.FromArgb("#2A2A2A") : Color.FromArgb("#F0F0F0"),
            TextColor = isDark ? Color.FromArgb("#AAAAAA") : Color.FromArgb("#666666"),
            FontSize = 14,
            CornerRadius = 12,
            HeightRequest = 46,
        };
        cancelButton.Clicked += (_, _) => ModalOverlay.Dismiss(this, null);

        var confirmButton = new Button
        {
            Text = "OK",
            BackgroundColor = accentColor,
            TextColor = accentText,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            CornerRadius = 12,
            HeightRequest = 46,
        };
        confirmButton.Clicked += (_, _) => ModalOverlay.Dismiss(this, _entry.Text?.Trim());

        var buttonGrid = new Grid
        {
            ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition { Width = new GridLength(12) }, new ColumnDefinition(GridLength.Star)],
            Margin = new Thickness(0, 12, 0, 0),
        };
        buttonGrid.Add(cancelButton, 0);
        buttonGrid.Add(confirmButton, 2);

        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Children.Add(titleLabel);
        stack.Children.Add(messageLabel);
        stack.Children.Add(_entry);
        stack.Children.Add(buttonGrid);

        Content = stack;
    }

    public Task<object?> ShowAsync(Page? page = null) => ModalOverlay.ShowAsync(this, page);
}
