namespace Golyath.UI.Controls;

/// <summary>
/// Shows modal overlays directly on the current page's visual tree.
/// No platform dialogs — just MAUI views with animations.
/// </summary>
public static class ModalOverlay
{
    public static Task<object?> ShowAsync(View content, Page? page = null)
    {
        page ??= Shell.Current.CurrentPage;
        var contentPage = page as ContentPage ?? throw new InvalidOperationException("ModalOverlay requires a ContentPage.");
        var tcs = new TaskCompletionSource<object?>();

        // Semi-transparent backdrop — explicit fill to cover entire screen
        var backdrop = new BoxView
        {
            Color = Color.FromArgb("#000000"),
            Opacity = 0,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };

        var container = new Grid
        {
            InputTransparent = false,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Children = { backdrop, content },
        };

        // Dismiss on backdrop tap
        var backdropTap = new TapGestureRecognizer();
        backdropTap.Tapped += (_, _) =>
        {
            DismissAsync(container, content, contentPage, tcs, null);
        };
        backdrop.GestureRecognizers.Add(backdropTap);

        // Attach dismiss callback to the content
        content.SetValue(DismissCallbackProperty, new Action<object?>(result =>
        {
            DismissAsync(container, content, contentPage, tcs, result);
        }));

        // Add overlay to page (keeps __ModalHost persistent for performance)
        InjectOverlay(contentPage, container);

        // Animate in
        backdrop.FadeTo(0.5, 200, Easing.CubicOut);
        content.TranslationY = 60;
        content.Opacity = 0;
        content.FadeTo(1, 200, Easing.CubicOut);
        content.TranslateTo(0, 0, 250, Easing.CubicOut);

        return tcs.Task;
    }

    internal static readonly BindableProperty DismissCallbackProperty =
        BindableProperty.CreateAttached("DismissCallback", typeof(Action<object?>), typeof(ModalOverlay), null);

    internal static void Dismiss(View content, object? result)
    {
        var callback = (Action<object?>?)content.GetValue(DismissCallbackProperty);
        callback?.Invoke(result);
    }

    private static async void DismissAsync(Grid container, View content, ContentPage page, TaskCompletionSource<object?> tcs, object? result)
    {
        if (tcs.Task.IsCompleted) return;

        // Animate out
        var t1 = ((VisualElement)container.Children[0]).FadeTo(0, 150, Easing.CubicIn);
        var t2 = content.FadeTo(0, 150, Easing.CubicIn);
        var t3 = content.TranslateTo(0, 40, 150, Easing.CubicIn);
        await Task.WhenAll(t1, t2, t3);

        RemoveOverlay(page, container);
        tcs.TrySetResult(result);
    }

    private static void InjectOverlay(ContentPage page, Grid overlay)
    {
        if (page.Content is Grid existingGrid && existingGrid.ClassId == "__ModalHost")
        {
            existingGrid.Children.Add(overlay);
        }
        else
        {
            var host = new Grid { ClassId = "__ModalHost" };
            var original = page.Content;
            page.Content = host;
            if (original is not null)
                host.Children.Add(original);
            host.Children.Add(overlay);
        }
    }

    private static void RemoveOverlay(ContentPage page, Grid overlay)
    {
        if (page.Content is Grid host && host.ClassId == "__ModalHost")
        {
            host.Children.Remove(overlay);
        }
    }
}
