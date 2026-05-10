using Golyath.Core.Enums;
using System.Globalization;

namespace Golyath.UI.Converters;

/// <summary>
/// Maps a <see cref="GoalType"/> to a background colour for type badge labels.
/// Strength → Amber, Frequency → Blue, Balance → Green.
/// </summary>
public sealed class GoalTypeBadgeColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GoalType type) return Color.FromArgb("#888888");

        return type switch
        {
            GoalType.Strength => Color.FromArgb("#F59E0B"),
            GoalType.Frequency => Color.FromArgb("#3B82F6"),
            GoalType.Balance => Color.FromArgb("#22C55E"),
            _ => Color.FromArgb("#888888")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
