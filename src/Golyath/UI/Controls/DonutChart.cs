namespace Golyath.UI.Controls;

/// <summary>
/// Data model for a single donut chart segment.
/// </summary>
public class DonutSegment
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public Color Color { get; set; } = Colors.Gray;

    public DonutSegment() { }
    public DonutSegment(string label, double value, Color color)
    {
        Label = label;
        Value = value;
        Color = color;
    }
}

/// <summary>
/// Pure-MAUI donut/pie chart with animated segment draw-in.
/// Segments sweep in sequentially from the top on first render.
/// Center is empty for XAML text overlay (e.g., total value).
/// </summary>
public class DonutChart : GraphicsView
{
    private readonly DonutDrawable _drawable = new();
    private IDispatcherTimer? _animTimer;
    private double _animElapsed;
    private const double AnimDurationMs = 900;
    private bool _hasAnimated;

    /// <summary>Default fitness-appropriate color palette.</summary>
    public static readonly Color[] DefaultPalette =
    [
        Color.FromArgb("#FFD700"), // Gold
        Color.FromArgb("#FFA500"), // Amber
        Color.FromArgb("#E8B960"), // Muted gold
        Color.FromArgb("#A0A0A0"), // Gray
        Color.FromArgb("#C0C0C0"), // Silver
        Color.FromArgb("#8B7355"), // Warm brown
        Color.FromArgb("#DAA520"), // Goldenrod
        Color.FromArgb("#BDB76B"), // Dark khaki
    ];

    // ── Segments ─────────────────────────────────────────────────────────────
    public static readonly BindableProperty SegmentsProperty = BindableProperty.Create(
        nameof(Segments),
        typeof(IReadOnlyList<DonutSegment>),
        typeof(DonutChart),
        defaultValue: null,
        propertyChanged: (b, _, n) =>
        {
            var chart = (DonutChart)b;
            chart._drawable.Segments = n as IReadOnlyList<DonutSegment> ?? [];
            chart._hasAnimated = false;
            chart.StartAnimation();
        });

    public IReadOnlyList<DonutSegment>? Segments
    {
        get => (IReadOnlyList<DonutSegment>?)GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    // ── Theme awareness ──────────────────────────────────────────────────────
    public static readonly BindableProperty IsDarkProperty = BindableProperty.Create(
        nameof(IsDark), typeof(bool), typeof(DonutChart), false,
        propertyChanged: (b, _, n) =>
        {
            var c = (DonutChart)b;
            c._drawable.IsDark = (bool)n;
            c.Invalidate();
        });

    public bool IsDark
    {
        get => (bool)GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    // ── Donut thickness ──────────────────────────────────────────────────────
    public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(
        nameof(StrokeWidth), typeof(float), typeof(DonutChart), 30f,
        propertyChanged: (b, _, n) =>
        {
            var c = (DonutChart)b;
            c._drawable.StrokeWidth = (float)n;
            c.Invalidate();
        });

    public float StrokeWidth
    {
        get => (float)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public DonutChart()
    {
        Drawable = _drawable;
    }

    // ── Animation ────────────────────────────────────────────────────────────
    private void StartAnimation()
    {
        _animTimer?.Stop();
        _animElapsed = 0;
        _drawable.AnimProgress = 0f;

        _animTimer = Dispatcher.CreateTimer();
        _animTimer.Interval = TimeSpan.FromMilliseconds(16);
        _animTimer.Tick += OnAnimTick;
        _animTimer.Start();
    }

    private void OnAnimTick(object? sender, EventArgs e)
    {
        _animElapsed += 16;
        double t = Math.Min(1.0, _animElapsed / AnimDurationMs);

        // Ease-out cubic
        double eased = 1.0 - Math.Pow(1.0 - t, 3);
        _drawable.AnimProgress = (float)eased;
        Invalidate();

        if (t >= 1.0)
        {
            _hasAnimated = true;
            _animTimer?.Stop();
            _animTimer = null;
        }
    }

    // ── Internal drawable ────────────────────────────────────────────────────

    private sealed class DonutDrawable : IDrawable
    {
        private const float GapDegrees = 2f; // gap between segments

        public IReadOnlyList<DonutSegment> Segments { get; set; } = [];
        public bool IsDark { get; set; }
        public float StrokeWidth { get; set; } = 30f;
        public float AnimProgress { get; set; } = 1f;

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;

            float sw = StrokeWidth;
            float cx = rect.Width / 2f;
            float cy = rect.Height / 2f;
            float radius = Math.Min(cx, cy) - sw / 2f - 4f;
            if (radius < 10f) return;

            float left = cx - radius;
            float top = cy - radius;
            float diameter = radius * 2f;

            // ── Empty state ──────────────────────────────────────────────────
            if (Segments.Count == 0)
            {
                var emptyColor = IsDark
                    ? Color.FromRgba(136, 136, 136, 51)
                    : Color.FromRgba(200, 200, 200, 77);
                canvas.StrokeColor = emptyColor;
                canvas.StrokeSize = sw;
                canvas.StrokeLineCap = LineCap.Butt;
                canvas.DrawArc(left, top, diameter, diameter, 0f, 359.9f, true, false);

                var textColor = IsDark
                    ? Color.FromRgba(153, 153, 153, 255)
                    : Color.FromRgba(120, 120, 120, 255);
                canvas.FontColor = textColor;
                canvas.FontSize = 13f;
                canvas.DrawString("No data", cx - 30f, cy - 8f, 60f, 16f,
                    HorizontalAlignment.Center, VerticalAlignment.Center);
                return;
            }

            // ── Calculate segment angles ─────────────────────────────────────
            double total = 0;
            foreach (var seg in Segments)
                total += Math.Max(0, seg.Value);
            if (total <= 0) return;

            int count = Segments.Count;
            float totalGap = count * GapDegrees;
            float available = 360f - totalGap;
            float animatedAvailable = available * AnimProgress;

            // Draw from 12 o'clock = 270° in MAUI coords, going clockwise (decreasing angle)
            float currentAngle = 270f;

            for (int i = 0; i < count; i++)
            {
                float segSweep = (float)(Segments[i].Value / total) * animatedAvailable;
                if (segSweep < 0.1f)
                {
                    currentAngle -= GapDegrees;
                    continue;
                }

                float segEnd = currentAngle - segSweep;

                canvas.StrokeColor = Segments[i].Color;
                canvas.StrokeSize = sw;
                canvas.StrokeLineCap = LineCap.Butt;
                canvas.DrawArc(left, top, diameter, diameter, currentAngle, segEnd, false, false);

                currentAngle = segEnd - GapDegrees;
            }
        }
    }
}
