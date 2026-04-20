using System.Globalization;

namespace Golyath.Converters;

/// <summary>
/// Converts bool to one of two strings from a "|"-separated ConverterParameter.
/// Parameter format: "TrueString|FalseString", e.g. "Compound|Isolation"
/// </summary>
public class BoolToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;
        string param = parameter as string ?? "|";
        var parts = param.Split('|');
        return flag ? parts[0] : (parts.Length > 1 ? parts[1] : string.Empty);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
