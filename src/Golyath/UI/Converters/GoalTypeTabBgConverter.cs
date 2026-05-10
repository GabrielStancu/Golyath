using System.Globalization;

namespace Golyath.UI.Converters;

/// <summary>
/// Returns the Accent background colour when the bound value matches the ConverterParameter,
/// otherwise Transparent. Used for the goal-type tab selector in AddGoalPage.
/// </summary>
public sealed class GoalTypeTabBgConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isSelected = value?.ToString() == parameter?.ToString();
        return isSelected
            ? Color.FromArgb("#FFD700")   // Accent
            : Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
