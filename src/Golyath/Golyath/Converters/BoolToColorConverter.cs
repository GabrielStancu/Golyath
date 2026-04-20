using System.Globalization;

namespace Golyath.Converters;

/// <summary>
/// Converts bool to one of two colors from a "|"-separated ConverterParameter string.
/// Parameter format: "TrueColor|FalseColor", e.g. "#F5C518|#282828"
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;
        string param = parameter as string ?? "#FFFFFF|#000000";
        var parts = param.Split('|');
        string hex = flag ? parts[0] : (parts.Length > 1 ? parts[1] : "#000000");
        return Color.FromArgb(hex);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
