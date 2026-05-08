using Golyath.Application.DTOs;

namespace Golyath.UI.Controls;

/// <summary>
/// Pure-MAUI GraphicsView that plots a line chart for strength-progression data.
/// X-axis = sessions (evenly spaced), Y-axis = weight. Gold line + dots.
/// Date labels are shown below every data point (abbreviated).
/// No third-party dependencies.
/// </summary>
public class LineChart : GraphicsView
{
    private readonly LineChartDrawable _drawable = new();

    // ── Points bindable property ─────────────────────────────────────────────

    public static readonly BindableProperty PointsProperty = BindableProperty.Create(
        nameof(Points),
        typeof(IReadOnlyList<StrengthPoint>),
        typeof(LineChart),
        defaultValue: null,
        propertyChanged: (b, _, n) =>
        {
            var c = (LineChart)b;
            c._drawable.Points = n as IReadOnlyList<StrengthPoint> ?? [];
            c.Invalidate();
        });

    public IReadOnlyList<StrengthPoint>? Points
    {
        get => (IReadOnlyList<StrengthPoint>?)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    // ── Dark-mode flag ───────────────────────────────────────────────────────

    public static readonly BindableProperty IsDarkProperty = BindableProperty.Create(
        nameof(IsDark),
        typeof(bool),
        typeof(LineChart),
        false,
        propertyChanged: (b, _, n) =>
        {
            var c = (LineChart)b;
            c._drawable.IsDark = (bool)n;
            c.Invalidate();
        });

    public bool IsDark
    {
        get => (bool)GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    public LineChart()
    {
        Drawable = _drawable;
    }

    // ── Drawable ─────────────────────────────────────────────────────────────

    private sealed class LineChartDrawable : IDrawable
    {
        private const float PadLeft   = 52f;
        private const float PadRight  = 12f;
        private const float PadTop    = 16f;
        private const float PadBottom = 36f;  // room for date labels

        public IReadOnlyList<StrengthPoint> Points { get; set; } = [];
        public bool IsDark { get; set; }

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;

            if (Points.Count == 0)
            {
                DrawEmptyState(canvas, rect);
                return;
            }

            float plotW = rect.Width  - PadLeft - PadRight;
            float plotH = rect.Height - PadTop  - PadBottom;

            double minY = Points.Min(p => p.MaxWeight);
            double maxY = Points.Max(p => p.MaxWeight);
            // Add headroom so the top dot isn't clipped
            double rangeY = maxY - minY;
            if (rangeY < 1) rangeY = 1;
            double yMin = Math.Max(0, minY - rangeY * 0.1);
            double yMax = maxY + rangeY * 0.1;
            double yRange = yMax - yMin;

            int n = Points.Count;

            // Helper: map a data point to canvas coordinates
            float MapX(int i) => PadLeft + (n == 1 ? plotW / 2f : i * plotW / (n - 1));
            float MapY(double w) => PadTop + plotH - (float)((w - yMin) / yRange * plotH);

            // ── Y-axis grid lines ────────────────────────────────────────────
            DrawYGridLines(canvas, rect, yMin, yMax, PadLeft, PadTop, plotW, plotH);

            // ── Line ─────────────────────────────────────────────────────────
            var accent = Color.FromArgb("#FFD700");
            canvas.StrokeColor = accent;
            canvas.StrokeSize  = 2.5f;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;

            var path = new PathF();
            for (int i = 0; i < n; i++)
            {
                float x = MapX(i);
                float y = MapY(Points[i].MaxWeight);
                if (i == 0) path.MoveTo(x, y);
                else        path.LineTo(x, y);
            }
            canvas.DrawPath(path);

            // ── Dots + date labels ───────────────────────────────────────────
            var labelColor = IsDark
                ? Color.FromArgb("#999999")
                : Color.FromArgb("#777777");

            for (int i = 0; i < n; i++)
            {
                float x = MapX(i);
                float y = MapY(Points[i].MaxWeight);

                // Outer ring
                canvas.FillColor   = Color.FromArgb("#2A2A2A");
                canvas.StrokeColor = accent;
                canvas.StrokeSize  = 2f;
                canvas.FillCircle(x, y, 5f);
                canvas.DrawCircle(x, y, 5f);

                // Date label — only for first, last and every ~4th point
                if (i == 0 || i == n - 1 || (n > 3 && i % Math.Max(1, n / 4) == 0))
                {
                    string label = Points[i].Date.ToLocalTime().ToString("M/d");
                    canvas.FontColor = labelColor;
                    canvas.FontSize  = 9f;
                    canvas.DrawString(label,
                        x - 20f, rect.Height - PadBottom + 4f,
                        40f, PadBottom - 4f,
                        HorizontalAlignment.Center,
                        VerticalAlignment.Top);
                }
            }
        }

        private void DrawYGridLines(ICanvas canvas, RectF rect,
            double yMin, double yMax,
            float padLeft, float padTop, float plotW, float plotH)
        {
            const int gridLines = 4;
            var gridColor = Color.FromRgba(0x88, 0x88, 0x88, 0x30);
            var labelColor = Color.FromArgb("#888888");

            for (int i = 0; i <= gridLines; i++)
            {
                double w  = yMin + (yMax - yMin) * i / gridLines;
                float  y  = padTop + plotH - plotH * (float)((w - yMin) / (yMax - yMin));

                canvas.StrokeColor = gridColor;
                canvas.StrokeSize  = 1f;
                canvas.DrawLine(padLeft, y, padLeft + plotW, y);

                // Weight label on the left
                canvas.FontColor = labelColor;
                canvas.FontSize  = 9f;
                canvas.DrawString(
                    w.ToString("0"),
                    0, y - 8f, padLeft - 4f, 16f,
                    HorizontalAlignment.Right,
                    VerticalAlignment.Center);
            }
        }

        private static void DrawEmptyState(ICanvas canvas, RectF rect)
        {
            canvas.FontColor = Color.FromArgb("#888888");
            canvas.FontSize  = 13f;
            canvas.DrawString(
                "No data for the selected period",
                rect.Left, rect.Top, rect.Width, rect.Height,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}
