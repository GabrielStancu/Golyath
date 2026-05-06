using System.Globalization;

namespace Golyath.UI.Converters;

/// <summary>Returns 1.0 opacity for true (active), 0.15 for false (inactive).</summary>
public sealed class BoolToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? 1.0 : 0.15;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
