using Golyath.Application.DTOs;

namespace Golyath.UI.Controls;

/// <summary>
/// Enhanced line chart with gradient fill, Bezier-curved lines, animated draw-in,
/// styled dots with a glowing last point, and subtle grid lines.
/// Pure MAUI GraphicsView — no third-party packages.
/// </summary>
public class AnimatedLineChart : GraphicsView
{
    private readonly AnimatedLineChartDrawable _drawable = new();
    private IDispatcherTimer? _animTimer;
    private double _animElapsed;
    private const double AnimDurationMs = 800;
    private bool _hasAppeared;

    // ── Points ───────────────────────────────────────────────────────────────
    public static readonly BindableProperty PointsProperty = BindableProperty.Create(
        nameof(Points),
        typeof(IReadOnlyList<StrengthPoint>),
        typeof(AnimatedLineChart),
        defaultValue: null,
        propertyChanged: (b, _, n) =>
        {
            var c = (AnimatedLineChart)b;
            c._drawable.Points = n as IReadOnlyList<StrengthPoint> ?? [];
            c._hasAppeared = false;
            if (c.AnimateOnAppear)
                c.StartDrawAnimation();
            else
            {
                c._drawable.ClipProgress = 1f;
                c.Invalidate();
            }
        });

    public IReadOnlyList<StrengthPoint>? Points
    {
        get => (IReadOnlyList<StrengthPoint>?)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    // ── Theme ────────────────────────────────────────────────────────────────
    public static readonly BindableProperty IsDarkProperty = BindableProperty.Create(
        nameof(IsDark), typeof(bool), typeof(AnimatedLineChart), false,
        propertyChanged: (b, _, n) =>
        {
            var c = (AnimatedLineChart)b;
            c._drawable.IsDark = (bool)n;
            c.Invalidate();
        });

    public bool IsDark
    {
        get => (bool)GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    // ── Animate on appear ────────────────────────────────────────────────────
    public static readonly BindableProperty AnimateOnAppearProperty = BindableProperty.Create(
        nameof(AnimateOnAppear), typeof(bool), typeof(AnimatedLineChart), true);

    public bool AnimateOnAppear
    {
        get => (bool)GetValue(AnimateOnAppearProperty);
        set => SetValue(AnimateOnAppearProperty, value);
    }

    public AnimatedLineChart()
    {
        Drawable = _drawable;
    }

    // ── Animation ────────────────────────────────────────────────────────────
    private void StartDrawAnimation()
    {
        _animTimer?.Stop();
        _animElapsed = 0;
        _drawable.ClipProgress = 0f;

        _animTimer = Dispatcher.CreateTimer();
        _animTimer.Interval = TimeSpan.FromMilliseconds(16);
        _animTimer.Tick += OnAnimTick;
        _animTimer.Start();
    }

    private void OnAnimTick(object? sender, EventArgs e)
    {
        _animElapsed += 16;
        double t = Math.Min(1.0, _animElapsed / AnimDurationMs);

        // Ease-out quad
        double eased = 1.0 - (1.0 - t) * (1.0 - t);
        _drawable.ClipProgress = (float)eased;
        Invalidate();

        if (t >= 1.0)
        {
            _hasAppeared = true;
            _animTimer?.Stop();
            _animTimer = null;
        }
    }

    // ── Internal drawable ────────────────────────────────────────────────────

    private sealed class AnimatedLineChartDrawable : IDrawable
    {
        private const float PadLeft = 52f;
        private const float PadRight = 16f;
        private const float PadTop = 20f;
        private const float PadBottom = 40f;

        private static readonly Color Accent = Color.FromArgb("#FFD700");

        public IReadOnlyList<StrengthPoint> Points { get; set; } = [];
        public bool IsDark { get; set; }
        /// <summary>0..1 progress of the left-to-right clip reveal.</summary>
        public float ClipProgress { get; set; } = 1f;

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;

            if (Points.Count == 0)
            {
                DrawEmptyState(canvas, rect);
                return;
            }

            float plotW = rect.Width - PadLeft - PadRight;
            float plotH = rect.Height - PadTop - PadBottom;
            if (plotW < 10 || plotH < 10) return;

            // ── Data range ───────────────────────────────────────────────────
            double minY = Points.Min(p => p.MaxWeight);
            double maxY = Points.Max(p => p.MaxWeight);
            double rangeY = maxY - minY;
            if (rangeY < 1) rangeY = 1;
            double yMin = Math.Max(0, minY - rangeY * 0.12);
            double yMax = maxY + rangeY * 0.12;
            double yRange = yMax - yMin;

            int n = Points.Count;
            float MapX(int i) => PadLeft + (n == 1 ? plotW / 2f : i * plotW / (n - 1));
            float MapY(double w) => PadTop + plotH - (float)((w - yMin) / yRange * plotH);

            // ── Clip region for animation (reveal left-to-right) ─────────────
            float clipRight = PadLeft + plotW * ClipProgress + 20f; // +20 so dots aren't cut
            canvas.SaveState();
            canvas.ClipRectangle(0, 0, clipRight, rect.Height);

            // ── Grid lines (horizontal dashed) ──────────────────────────────
            DrawGridLines(canvas, yMin, yMax, yRange, plotW, plotH);

            // ── Build point coordinates ──────────────────────────────────────
            float[] xs = new float[n];
            float[] ys = new float[n];
            for (int i = 0; i < n; i++)
            {
                xs[i] = MapX(i);
                ys[i] = MapY(Points[i].MaxWeight);
            }

            // ── Gradient fill under the curve ────────────────────────────────
            DrawGradientFill(canvas, xs, ys, n, PadTop + plotH);

            // ── Bezier curved line ───────────────────────────────────────────
            DrawCurvedLine(canvas, xs, ys, n);

            // ── Dots ─────────────────────────────────────────────────────────
            DrawDots(canvas, xs, ys, n);

            canvas.RestoreState();

            // ── Y-axis labels (outside clip so always visible) ───────────────
            DrawYLabels(canvas, yMin, yMax, yRange, plotH);

            // ── X-axis date labels ───────────────────────────────────────────
            DrawXLabels(canvas, rect, xs, n);
        }

        private void DrawEmptyState(ICanvas canvas, RectF rect)
        {
            var textColor = IsDark
                ? Color.FromRgba(153, 153, 153, 255)
                : Color.FromRgba(120, 120, 120, 255);
            canvas.FontColor = textColor;
            canvas.FontSize = 13f;
            canvas.DrawString("No data yet",
                rect.Width / 2f - 40f, rect.Height / 2f - 8f, 80f, 16f,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        private void DrawGridLines(ICanvas canvas, double yMin, double yMax, double yRange, float plotW, float plotH)
        {
            var gridColor = IsDark
                ? Color.FromRgba(255, 255, 255, 20)  // rgba(255,255,255,0.08)
                : Color.FromRgba(0, 0, 0, 15);       // rgba(0,0,0,0.06)

            // Draw 4-5 horizontal grid lines
            int gridCount = 4;
            canvas.StrokeColor = gridColor;
            canvas.StrokeSize = 1f;

            for (int i = 0; i <= gridCount; i++)
            {
                float y = PadTop + plotH * i / gridCount;

                // Draw as dashed line (manual short segments)
                float dashLen = 5f;
                float gapLen = 4f;
                float x = PadLeft;
                while (x < PadLeft + plotW)
                {
                    float end = Math.Min(x + dashLen, PadLeft + plotW);
                    canvas.DrawLine(x, y, end, y);
                    x = end + gapLen;
                }
            }
        }

        private void DrawGradientFill(ICanvas canvas, float[] xs, float[] ys, int n, float bottomY)
        {
            if (n < 2) return;

            // Build a path: curve along the top, then straight line along the bottom
            var fillPath = new PathF();
            fillPath.MoveTo(xs[0], ys[0]);

            // Use the same Bezier interpolation as the line
            for (int i = 0; i < n - 1; i++)
            {
                float x0 = xs[i], y0 = ys[i];
                float x1 = xs[i + 1], y1 = ys[i + 1];
                float cp = (x1 - x0) * 0.4f;
                fillPath.CurveTo(x0 + cp, y0, x1 - cp, y1, x1, y1);
            }

            // Close along the bottom
            fillPath.LineTo(xs[n - 1], bottomY);
            fillPath.LineTo(xs[0], bottomY);
            fillPath.Close();

            // Gradient: gold at top, transparent at bottom
            var gradTop = Accent.WithAlpha(0.35f);
            var gradBottom = Accent.WithAlpha(0.02f);

            float minYCoord = ys.Min();
            var gradientPaint = new LinearGradientPaint
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops = new[]
                {
                    new PaintGradientStop(0f, gradTop),
                    new PaintGradientStop(1f, gradBottom),
                }
            };
            canvas.SetFillPaint(gradientPaint,
                new RectF(xs[0], minYCoord, xs[n - 1] - xs[0], bottomY - minYCoord));

            canvas.FillPath(fillPath);
        }

        private void DrawCurvedLine(ICanvas canvas, float[] xs, float[] ys, int n)
        {
            if (n < 2) return;

            canvas.StrokeColor = Accent;
            canvas.StrokeSize = 2.8f;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;

            var path = new PathF();
            path.MoveTo(xs[0], ys[0]);

            for (int i = 0; i < n - 1; i++)
            {
                float x0 = xs[i], y0 = ys[i];
                float x1 = xs[i + 1], y1 = ys[i + 1];
                float cp = (x1 - x0) * 0.4f; // control point offset for smooth cubic Bezier
                path.CurveTo(x0 + cp, y0, x1 - cp, y1, x1, y1);
            }

            canvas.DrawPath(path);
        }

        private void DrawDots(ICanvas canvas, float[] xs, float[] ys, int n)
        {
            float dotRadius = 6f;
            var darkFill = IsDark
                ? Color.FromArgb("#1E1E1E")
                : Color.FromArgb("#FFFFFF");

            for (int i = 0; i < n; i++)
            {
                float x = xs[i];
                float y = ys[i];
                bool isLast = i == n - 1;

                if (isLast && ClipProgress >= 0.98f)
                {
                    // Pulse/glow on last dot
                    canvas.FillColor = Accent.WithAlpha(0.15f);
                    canvas.FillCircle(x, y, dotRadius * 2.2f);
                    canvas.FillColor = Accent.WithAlpha(0.3f);
                    canvas.FillCircle(x, y, dotRadius * 1.5f);
                }

                // Dark fill circle
                canvas.FillColor = darkFill;
                canvas.FillCircle(x, y, dotRadius);

                // Gold border ring
                canvas.StrokeColor = Accent;
                canvas.StrokeSize = 2.5f;
                canvas.DrawCircle(x, y, dotRadius);
            }
        }

        private void DrawYLabels(ICanvas canvas, double yMin, double yMax, double yRange, float plotH)
        {
            var labelColor = IsDark
                ? Color.FromArgb("#888888")
                : Color.FromArgb("#777777");

            canvas.FontColor = labelColor;
            canvas.FontSize = 9f;

            int gridCount = 4;
            for (int i = 0; i <= gridCount; i++)
            {
                float y = PadTop + plotH * i / gridCount;
                double val = yMax - (yMax - yMin) * i / gridCount;
                string label = val >= 100 ? $"{val:F0}" : $"{val:F1}";
                canvas.DrawString(label, 2f, y - 6f, PadLeft - 6f, 12f,
                    HorizontalAlignment.Right, VerticalAlignment.Center);
            }
        }

        private void DrawXLabels(ICanvas canvas, RectF rect, float[] xs, int n)
        {
            var labelColor = IsDark
                ? Color.FromArgb("#888888")
                : Color.FromArgb("#777777");

            canvas.FontColor = labelColor;
            canvas.FontSize = 9f;

            for (int i = 0; i < n; i++)
            {
                // Show first, last, and evenly spaced labels
                if (i != 0 && i != n - 1 && (n <= 3 || i % Math.Max(1, n / 4) != 0))
                    continue;

                string label = Points[i].Date.ToLocalTime().ToString("M/d");
                canvas.DrawString(label,
                    xs[i] - 22f, rect.Height - PadBottom + 6f,
                    44f, PadBottom - 6f,
                    HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }
    }
}
