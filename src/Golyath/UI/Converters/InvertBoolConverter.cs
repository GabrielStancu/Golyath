using System.Globalization;

namespace Golyath.UI.Converters;

/// <summary>Returns the logical NOT of a boolean binding value.</summary>
public sealed class InvertBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;
}
