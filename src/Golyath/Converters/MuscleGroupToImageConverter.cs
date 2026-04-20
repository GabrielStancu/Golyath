using System.Globalization;

namespace Golyath.Converters;

/// <summary>
/// Maps a muscle group display name to the corresponding SVG resource path
/// for use on the ExerciseDetailPage hero image.
/// </summary>
public class MuscleGroupToImageConverter : IValueConverter
{
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Chest",     "muscle_chest.png" },
        { "Back",      "muscle_back.png" },
        { "Shoulders", "muscle_shoulders.png" },
        { "Shoulder",  "muscle_shoulders.png" },
        { "Biceps",    "muscle_biceps.png" },
        { "Triceps",   "muscle_triceps.png" },
        { "Arms",      "muscle_biceps.png" },
        { "Legs",      "muscle_legs.png" },
        { "Quads",     "muscle_legs.png" },
        { "Hamstrings","muscle_legs.png" },
        { "Glutes",    "muscle_glutes.png" },
        { "Core",      "muscle_core.png" },
        { "Abs",       "muscle_core.png" },
        { "Calves",    "muscle_calves.png" },
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string name = value as string ?? string.Empty;
        if (Map.TryGetValue(name, out string? path))
            return path;
        // Default fallback
        return "muscle_chest.png";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
