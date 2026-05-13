using CommunityToolkit.Maui;
using Golyath.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Golyath;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                // Remove underline + set caret color for Entry controls
                EntryHandler.Mapper.AppendToMapping("EntryCustomization", (handler, view) =>
                {
                    handler.PlatformView.BackgroundTintList =
                        Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);

                    var isDark = App.Current?.RequestedTheme == AppTheme.Dark;
                    var caretColor = isDark
                        ? Android.Graphics.Color.Rgb(255, 215, 0) // Gold (#FFD700)
                        : Android.Graphics.Color.Black;
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
                    {
                        var cursorDrawable = handler.PlatformView.TextCursorDrawable;
                        cursorDrawable?.SetTint(caretColor);
                        handler.PlatformView.TextCursorDrawable = cursorDrawable;
                    }
                    handler.PlatformView.SetHighlightColor(caretColor);
                });

                // Remove underline from Picker controls
                PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                {
                    handler.PlatformView.BackgroundTintList =
                        Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                });

                // Remove underline from SearchBar controls
                SearchBarHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                {
                    var editText = handler.PlatformView.GetChildrenOfType<Android.Widget.EditText>().FirstOrDefault();
                    if (editText != null)
                    {
                        editText.BackgroundTintList =
                            Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
                    }
                });
#elif IOS || MACCATALYST
                // Set caret (tint) color for Entry on iOS
                EntryHandler.Mapper.AppendToMapping("EntryCustomization", (handler, view) =>
                {
                    var isDark = App.Current?.RequestedTheme == AppTheme.Dark;
                    handler.PlatformView.TintColor = isDark
                        ? UIKit.UIColor.FromRGB(255, 215, 0) // Gold
                        : UIKit.UIColor.Black;
                });
#endif
            });

        builder.Services.AddInfrastructure();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
