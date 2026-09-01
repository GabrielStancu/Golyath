using Golyath.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Platform;
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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Inter-Regular.ttf", "InterRegular");
                fonts.AddFont("Inter-SemiBold.ttf", "InterSemibold");
                fonts.AddFont("Inter-Bold.ttf", "InterBold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
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

                // Remove underline + set caret color for Editor controls
                EditorHandler.Mapper.AppendToMapping("EditorCustomization", (handler, view) =>
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

                // Remove underline from DatePicker; show a runtime-themed dialog so dark/light
                // mode is resolved at tap-time (values-night/ is skipped because UiMode is in
                // ConfigurationChanges and the activity is never recreated on theme switch).
                DatePickerHandler.Mapper.AppendToMapping("DatePickerCustomization", (handler, view) =>
                {
                    handler.PlatformView.SetBackground(
                        new Android.Graphics.Drawables.ColorDrawable(Android.Graphics.Color.Transparent));

                    Android.App.DatePickerDialog? activeDialog = null;

                    handler.PlatformView.ShowPicker = () =>
                    {
                        try { activeDialog?.Dismiss(); } catch { }
                        activeDialog = null;

                        var datePicker = handler.VirtualView;
                        if (datePicker == null) return;

                        var isDark = App.Current?.RequestedTheme == AppTheme.Dark;
                        var themeResId = isDark
                            ? Resource.Style.GolyathDatePickerDialogDark
                            : Resource.Style.GolyathDatePickerDialog;

                        var date = datePicker.Date;
                        activeDialog = new Android.App.DatePickerDialog(
                            handler.PlatformView.Context,
                            themeResId,
                            (Android.App.DatePickerDialog.IOnDateSetListener?)null,
                            date.Year,
                            date.Month - 1,   // Android month is 0-indexed
                            date.Day);

                        activeDialog.DateSet += (_, args) =>
                        {
                            datePicker.Date = new DateTime(args.Year, args.Month + 1, args.DayOfMonth);
                        };

                        if (datePicker.MinimumDate != DateTime.MinValue)
                            activeDialog.DatePicker.MinDate = (long)(datePicker.MinimumDate.ToUniversalTime()
                                - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;

                        if (datePicker.MaximumDate != DateTime.MaxValue)
                            activeDialog.DatePicker.MaxDate = (long)(datePicker.MaximumDate.ToUniversalTime()
                                - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                        // android:textColorPrimary makes calendar numbers white but also
                        // turns the header date white. Fix the header programmatically
                        // before Show() so there is no flicker.
                        if (isDark)
                        {
                            void DarkenViews(Android.Views.ViewGroup vg)
                            {
                                var dark = Android.Graphics.Color.ParseColor("#111111");
                                for (int i = 0; i < vg.ChildCount; i++)
                                {
                                    if (vg.GetChildAt(i) is Android.Widget.TextView tv)
                                        tv.SetTextColor(dark);
                                    else if (vg.GetChildAt(i) is Android.Views.ViewGroup inner)
                                        DarkenViews(inner);
                                }
                            }
                            // The header is the first child of the DatePicker widget.
                            var dp = activeDialog.DatePicker;
                            if (dp.ChildCount > 0 && dp.GetChildAt(0) is Android.Views.ViewGroup headerGroup)
                                DarkenViews(headerGroup);
                        }
                        activeDialog.Show();

                        // Theme attributes don't reach DatePickerDialog buttons —
                        // set the OK / Cancel text colour programmatically.
                        if (isDark)
                        {
                            var gold = Android.Graphics.Color.ParseColor("#FFD700");
                            activeDialog.GetButton(-1)?.SetTextColor(gold); // BUTTON_POSITIVE
                            activeDialog.GetButton(-2)?.SetTextColor(gold); // BUTTON_NEGATIVE
                        }
                    };

                    handler.PlatformView.HidePicker = () =>
                    {
                        try { activeDialog?.Dismiss(); } catch { }
                        activeDialog = null;
                    };
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

                // Use bounded ripple for Labels with TapGestureRecognizer (back buttons, chevrons)
                // Prevents the oversized borderless circle ripple
                LabelHandler.Mapper.AppendToMapping("BoundedTapRipple", (handler, view) =>
                {
                    if (view is Label label && label.GestureRecognizers.OfType<TapGestureRecognizer>().Any())
                    {
                        handler.PlatformView.Clickable = true;
                        var outValue = new Android.Util.TypedValue();
                        handler.PlatformView.Context?.Theme?.ResolveAttribute(
                            Android.Resource.Attribute.SelectableItemBackground, outValue, true);
                        handler.PlatformView.SetBackgroundResource(outValue.ResourceId);
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
