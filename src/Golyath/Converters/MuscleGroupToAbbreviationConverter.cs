using System.Globalization;

namespace Golyath.Converters;

/// <summary>
/// Converts a muscle group display name to a short 2–3 letter badge abbreviation.
/// Example: "Chest" → "CH", "Shoulders" → "SH"
/// </summary>
public class MuscleGroupToAbbreviationConverter : IValueConverter
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Chest",     "CH" },
        { "Back",      "BK" },
        { "Shoulders", "SH" },
        { "Shoulder",  "SH" },
        { "Biceps",    "BI" },
        { "Triceps",   "TR" },
        { "Arms",      "AR" },
        { "Legs",      "LG" },
        { "Quads",     "QD" },
        { "Hamstrings","HS" },
        { "Glutes",    "GL" },
        { "Core",      "CO" },
        { "Abs",       "AB" },
        { "Calves",    "CV" },
        { "Full Body", "FB" },
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string name = value as string ?? string.Empty;
        if (Map.TryGetValue(name, out string? abbr))
            return abbr;
        // Generate from first 2 characters if no mapping
        return name.Length >= 2 ? name[..2].ToUpperInvariant() : name.ToUpperInvariant();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
