using System.Globalization;

namespace Golyath.UI.Converters;

/// <summary>
/// Returns dark text (#111111) when the bound value matches the ConverterParameter (selected tab),
/// otherwise muted grey (#888888). Used for the goal-type tab selector in AddGoalPage.
/// </summary>
public sealed class GoalTypeTabTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isSelected = value?.ToString() == parameter?.ToString();
        return isSelected
            ? Color.FromArgb("#111111")   // AccentText
            : Color.FromArgb("#888888");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
