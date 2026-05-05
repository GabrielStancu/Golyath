using System.Globalization;

namespace Golyath.UI.Converters;

/// <summary>Returns true when the bound string is not null or whitespace.</summary>
public sealed class IsNotEmptyStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
