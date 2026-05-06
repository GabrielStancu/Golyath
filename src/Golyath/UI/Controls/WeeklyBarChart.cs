using Golyath.Application.DTOs;

namespace Golyath.UI.Controls;

/// <summary>
/// Pure-MAUI GraphicsView rendering a 7-day workout activity bar chart.
/// Gold bars for workout days, translucent stubs for rest days.
/// Works on all platforms without any third-party dependencies.
/// </summary>
public class WeeklyBarChart : GraphicsView
{
    private readonly ChartDrawable _drawable = new();

    public static readonly BindableProperty DaysProperty = BindableProperty.Create(
        nameof(Days),
        typeof(IReadOnlyList<WeeklyActivityDay>),
        typeof(WeeklyBarChart),
        defaultValue: null,
        propertyChanged: (b, _, n) =>
        {
            var c = (WeeklyBarChart)b;
            c._drawable.Days = n as IReadOnlyList<WeeklyActivityDay> ?? [];
            c.Invalidate();
        });

    public IReadOnlyList<WeeklyActivityDay>? Days
    {
        get => (IReadOnlyList<WeeklyActivityDay>?)GetValue(DaysProperty);
        set => SetValue(DaysProperty, value);
    }

    public WeeklyBarChart()
    {
        Drawable = _drawable;
    }

    // ── Internal drawable ────────────────────────────────────────────────────

    private sealed class ChartDrawable : IDrawable
    {
        private const float LabelZoneH = 20f;
        private const float SlotPad = 0.18f;   // fraction of slot width used as padding each side
        private const float BarMinH = 6f;       // minimum bar height for rest-day stub

        public IReadOnlyList<WeeklyActivityDay> Days { get; set; } = [];

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;
            if (Days.Count == 0) return;

            int n = Days.Count;
            float slotW = rect.Width / n;
            float barAreaH = rect.Height - LabelZoneH;

            for (int i = 0; i < n; i++)
            {
                var day = Days[i];
                float sx = i * slotW;
                float pad = slotW * SlotPad;
                float bx = sx + pad;
                float bw = slotW - pad * 2f;
                float cr = bw * 0.38f;       // corner radius (pill-shaped)

                // Rest-day stub (dim, short) — always drawn as background track
                float stubH = BarMinH + (barAreaH - BarMinH) * 0.18f;
                float stubY = barAreaH - stubH;
                canvas.FillColor = Color.FromRgba(0x88, 0x88, 0x88, 0x38);
                canvas.FillRoundedRectangle(bx, stubY, bw, stubH, cr);

                // Workout bar — full height, gold
                if (day.HasWorkout)
                {
                    canvas.FillColor = Color.FromArgb("#FFD700");
                    canvas.FillRoundedRectangle(bx, 0, bw, barAreaH, cr);
                }

                // Day label — gold for today, gray for others
                canvas.FontColor = day.IsToday
                    ? Color.FromArgb("#FFD700")
                    : Color.FromArgb("#777777");
                canvas.FontSize = 10;
                canvas.DrawString(
                    day.Label,
                    sx, barAreaH, slotW, LabelZoneH,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Center);
            }
        }
    }
}
