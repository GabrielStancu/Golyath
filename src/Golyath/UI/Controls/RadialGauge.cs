namespace Golyath.UI.Controls;

/// <summary>
/// Full 360° circular gauge with animated fill, decorative tick marks,
/// and a glow effect at the leading edge. Pure MAUI GraphicsView — no third-party packages.
///
/// The arc fills clockwise from 12 o'clock (top). Overlay text/emoji via XAML Grid.
/// </summary>
public class RadialGauge : GraphicsView
{
    private readonly GaugeDrawable _drawable = new();
    private IDispatcherTimer? _animTimer;
    private double _animFrom;
    private double _animTo;
    private double _animElapsed;
    private double _animDurationMs;

    // ── Value (0 – 1) ────────────────────────────────────────────────────────
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double), typeof(RadialGauge), 0.0,
        propertyChanged: OnValueChanged);

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // ── Arc color ────────────────────────────────────────────────────────────
    public static readonly BindableProperty GaugeColorProperty = BindableProperty.Create(
        nameof(GaugeColor), typeof(Color), typeof(RadialGauge), Color.FromArgb("#FFD700"),
        propertyChanged: Refresh<Color>((g, v) => g._drawable.GaugeColor = v));

    public Color GaugeColor
    {
        get => (Color)GetValue(GaugeColorProperty);
        set => SetValue(GaugeColorProperty, value);
    }

    // ── Track thickness ──────────────────────────────────────────────────────
    public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(
        nameof(StrokeWidth), typeof(float), typeof(RadialGauge), 18f,
        propertyChanged: Refresh<float>((g, v) => g._drawable.StrokeWidth = v));

    public float StrokeWidth
    {
        get => (float)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    // ── Theme awareness ──────────────────────────────────────────────────────
    public static readonly BindableProperty IsDarkProperty = BindableProperty.Create(
        nameof(IsDark), typeof(bool), typeof(RadialGauge), false,
        propertyChanged: Refresh<bool>((g, v) => g._drawable.IsDark = v));

    public bool IsDark
    {
        get => (bool)GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    // ── Animation duration (ms) ──────────────────────────────────────────────
    public static readonly BindableProperty AnimationDurationProperty = BindableProperty.Create(
        nameof(AnimationDuration), typeof(int), typeof(RadialGauge), 600);

    public int AnimationDuration
    {
        get => (int)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public RadialGauge()
    {
        Drawable = _drawable;
    }

    // ── Value changed → start animation ──────────────────────────────────────
    private static void OnValueChanged(BindableObject b, object oldVal, object newVal)
    {
        var gauge = (RadialGauge)b;
        double from = (double)oldVal;
        double to = (double)newVal;

        gauge.StartAnimation(from, to);
    }

    private void StartAnimation(double from, double to)
    {
        _animTimer?.Stop();

        _animFrom = from;
        _animTo = to;
        _animElapsed = 0;
        _animDurationMs = Math.Max(1, AnimationDuration);

        const double intervalMs = 16; // ~60fps
        _animTimer = Dispatcher.CreateTimer();
        _animTimer.Interval = TimeSpan.FromMilliseconds(intervalMs);
        _animTimer.Tick += OnAnimTick;
        _animTimer.Start();
    }

    private void OnAnimTick(object? sender, EventArgs e)
    {
        _animElapsed += 16;
        double t = Math.Min(1.0, _animElapsed / _animDurationMs);

        // Ease-out cubic for a smooth deceleration feel
        double eased = 1.0 - Math.Pow(1.0 - t, 3);
        double current = _animFrom + (_animTo - _animFrom) * eased;

        _drawable.DisplayValue = Math.Clamp(current, 0.0, 1.0);
        Invalidate();

        if (t >= 1.0)
        {
            _animTimer?.Stop();
            _animTimer = null;
        }
    }

    // ── Helper for simple property-changed refresh ───────────────────────────
    private static BindableProperty.BindingPropertyChangedDelegate Refresh<T>(Action<RadialGauge, T> apply) =>
        (b, _, n) =>
        {
            var gauge = (RadialGauge)b;
            apply(gauge, (T)n);
            gauge.Invalidate();
        };

    // ── Internal drawable ────────────────────────────────────────────────────

    private sealed class GaugeDrawable : IDrawable
    {
        // Number of decorative tick marks around the perimeter
        private const int TickCount = 24;

        /// <summary>Current display value (animated). Set by the timer.</summary>
        public double DisplayValue { get; set; }
        public Color GaugeColor { get; set; } = Color.FromArgb("#FFD700");
        public float StrokeWidth { get; set; } = 18f;
        public bool IsDark { get; set; }

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;

            float sw = StrokeWidth;
            float cx = rect.Width / 2f;
            float cy = rect.Height / 2f;
            float radius = Math.Min(cx, cy) - sw - 6f; // 6px extra margin for glow & ticks
            if (radius < 10f) return;

            float left = cx - radius;
            float top = cy - radius;
            float diameter = radius * 2f;

            // ── Track colors based on theme ──────────────────────────────────
            var trackColor = IsDark
                ? Color.FromRgba(136, 136, 136, 51)   // rgba(136,136,136,0.2)
                : Color.FromRgba(200, 200, 200, 77);  // rgba(200,200,200,0.3)

            // ── Background track (full 360°) ─────────────────────────────────
            canvas.StrokeColor = trackColor;
            canvas.StrokeSize = sw;
            canvas.StrokeLineCap = LineCap.Butt;
            canvas.DrawArc(left, top, diameter, diameter, 0f, 359.9f, true, false);

            // ── Decorative tick marks ────────────────────────────────────────
            float tickOuterRadius = radius + sw * 0.5f + 3f;
            float tickDotRadius = 1.5f;
            var tickColor = IsDark
                ? Color.FromRgba(255, 255, 255, 38)  // ~15% white
                : Color.FromRgba(0, 0, 0, 30);       // ~12% black

            canvas.FillColor = tickColor;
            for (int i = 0; i < TickCount; i++)
            {
                double angleRad = 2.0 * Math.PI * i / TickCount - Math.PI / 2.0; // start from top
                float tx = cx + (float)(tickOuterRadius * Math.Cos(angleRad));
                float ty = cy + (float)(tickOuterRadius * Math.Sin(angleRad));
                canvas.FillCircle(tx, ty, tickDotRadius);
            }

            // ── Value arc ────────────────────────────────────────────────────
            float clamped = (float)Math.Clamp(DisplayValue, 0.0, 1.0);
            if (clamped > 0.005f)
            {
                // Arc from 12 o'clock clockwise.
                // MAUI angles: 0°=3 o'clock, clockwise. 12 o'clock = 270°.
                float sweepDeg = clamped * 359.9f;
                float arcStart = 270f;
                float arcEnd = arcStart - sweepDeg;

                canvas.StrokeColor = GaugeColor;
                canvas.StrokeSize = sw;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.DrawArc(left, top, diameter, diameter, arcStart, arcEnd, false, false);

                // ── Glow effect at leading edge ──────────────────────────────
                double leadAngleRad = (-90.0 + sweepDeg) * Math.PI / 180.0; // from top, CW
                float glowX = cx + (float)(radius * Math.Cos(leadAngleRad));
                float glowY = cy + (float)(radius * Math.Sin(leadAngleRad));

                // Outer glow (larger, semi-transparent)
                float glowR1 = sw * 0.9f;
                canvas.FillColor = GaugeColor.WithAlpha(0.15f);
                canvas.FillCircle(glowX, glowY, glowR1);

                // Inner glow
                float glowR2 = sw * 0.55f;
                canvas.FillColor = GaugeColor.WithAlpha(0.3f);
                canvas.FillCircle(glowX, glowY, glowR2);
            }
        }
    }
}
