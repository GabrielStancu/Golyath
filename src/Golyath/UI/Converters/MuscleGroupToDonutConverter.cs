using System.Globalization;
using Golyath.Application.DTOs;
using Golyath.UI.Controls;

namespace Golyath.UI.Converters;

/// <summary>
/// Converts IReadOnlyList&lt;MuscleGroupVolume&gt; to IReadOnlyList&lt;DonutSegment&gt;
/// for use with the DonutChart control.
/// </summary>
public class MuscleGroupToDonutConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IReadOnlyList<MuscleGroupVolume> muscles || muscles.Count == 0)
            return Array.Empty<DonutSegment>();

        var segments = new List<DonutSegment>();
        for (int i = 0; i < muscles.Count; i++)
        {
            var m = muscles[i];
            var color = DonutChart.DefaultPalette[i % DonutChart.DefaultPalette.Length];
            segments.Add(new DonutSegment(m.MuscleGroup, m.SetCount, color));
        }
        return segments.AsReadOnly();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
