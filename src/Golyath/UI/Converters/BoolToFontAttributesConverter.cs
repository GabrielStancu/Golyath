using System.Globalization;

namespace Golyath.UI.Converters;

/// <summary>Returns Bold FontAttributes for true, None for false.</summary>
public sealed class BoolToFontAttributesConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? FontAttributes.Bold : FontAttributes.None;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
