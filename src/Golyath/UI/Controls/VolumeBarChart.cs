using Golyath.Application.DTOs;

namespace Golyath.UI.Controls;

/// <summary>
/// Pure-MAUI GraphicsView that renders a vertical bar chart for weekly training volume.
/// Bars are gold-filled; week labels appear below each bar.
/// No third-party dependencies.
/// </summary>
public class VolumeBarChart : GraphicsView
{
    private readonly VolumeDrawable _drawable = new();

    public static readonly BindableProperty PointsProperty = BindableProperty.Create(
        nameof(Points),
        typeof(IReadOnlyList<VolumePoint>),
        typeof(VolumeBarChart),
        defaultValue: null,
        propertyChanged: (b, _, n) =>
        {
            var c = (VolumeBarChart)b;
            c._drawable.Points = n as IReadOnlyList<VolumePoint> ?? [];
            c.Invalidate();
        });

    public IReadOnlyList<VolumePoint>? Points
    {
        get => (IReadOnlyList<VolumePoint>?)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public VolumeBarChart()
    {
        Drawable = _drawable;
    }

    // ── Drawable ─────────────────────────────────────────────────────────────

    private sealed class VolumeDrawable : IDrawable
    {
        private const float PadLeft   = 52f;
        private const float PadRight  = 8f;
        private const float PadTop    = 8f;
        private const float LabelZoneH = 28f;
        private const float SlotPad    = 0.14f;

        public IReadOnlyList<VolumePoint> Points { get; set; } = [];

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;

            if (Points.Count == 0)
            {
                canvas.FontColor = Color.FromArgb("#888888");
                canvas.FontSize  = 13f;
                canvas.DrawString(
                    "No data for the selected period",
                    rect.Left, rect.Top, rect.Width, rect.Height,
                    HorizontalAlignment.Center, VerticalAlignment.Center);
                return;
            }

            double maxVol = Points.Max(p => p.Volume);
            if (maxVol < 1) maxVol = 1;

            float plotW  = rect.Width - PadLeft - PadRight;
            float plotH  = rect.Height - PadTop - LabelZoneH;

            // Y-axis grid lines (4 levels)
            DrawYGrid(canvas, rect, maxVol, PadLeft, PadTop, plotW, plotH);

            int   n      = Points.Count;
            float slotW  = plotW / n;
            var   accent = Color.FromArgb("#FFD700");

            for (int i = 0; i < n; i++)
            {
                var   p    = Points[i];
                float pad  = slotW * SlotPad;
                float bx   = PadLeft + i * slotW + pad;
                float bw   = slotW - pad * 2f;
                float bh   = (float)(p.Volume / maxVol * plotH);
                float by   = PadTop + plotH - bh;
                float cr   = bw * 0.25f;

                // Track stub
                canvas.FillColor = Color.FromRgba(0x88, 0x88, 0x88, 0x28);
                float stubH = Math.Max(4f, plotH * 0.06f);
                canvas.FillRoundedRectangle(bx, PadTop + plotH - stubH, bw, stubH, cr);

                // Filled bar
                if (bh > 2f)
                {
                    canvas.FillColor = accent;
                    canvas.FillRoundedRectangle(bx, by, bw, bh, cr);
                }

                // Week label — show every label if few weeks, otherwise skip middle ones
                bool showLabel = n <= 8 || i == 0 || i == n - 1 || i % Math.Max(1, n / 4) == 0;
                if (showLabel)
                {
                    canvas.FontColor = Color.FromArgb("#777777");
                    canvas.FontSize  = 9f;
                    float sx = PadLeft + i * slotW;
                    canvas.DrawString(
                        p.Label,
                        sx, PadTop + plotH + 2f,
                        slotW, LabelZoneH - 2f,
                        HorizontalAlignment.Center,
                        VerticalAlignment.Top);
                }
            }
        }

        private static void DrawYGrid(ICanvas canvas, RectF rect,
            double maxVol, float padLeft, float padTop, float plotW, float plotH)
        {
            const int lines = 4;
            var gridColor  = Color.FromRgba(0x88, 0x88, 0x88, 0x28);
            var labelColor = Color.FromArgb("#888888");

            for (int i = 0; i <= lines; i++)
            {
                double vol = maxVol * i / lines;
                float  y   = padTop + plotH - plotH * (float)(vol / maxVol);

                canvas.StrokeColor = gridColor;
                canvas.StrokeSize  = 1f;
                canvas.DrawLine(padLeft, y, padLeft + plotW, y);

                canvas.FontColor = labelColor;
                canvas.FontSize  = 9f;
                string label = vol >= 1000 ? $"{vol / 1000:0.#}k" : vol.ToString("0");
                canvas.DrawString(label, 0, y - 8f, padLeft - 4f, 16f,
                    HorizontalAlignment.Right, VerticalAlignment.Center);
            }
        }
    }
}
