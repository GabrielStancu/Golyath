using System.Globalization;

namespace Golyath.UI.Converters;

/// <summary>Returns true when the bound integer is greater than zero.</summary>
public sealed class IsNotZeroConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int n && n > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
