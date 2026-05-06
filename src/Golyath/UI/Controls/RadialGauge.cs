namespace Golyath.UI.Controls;

/// <summary>
/// Pure-MAUI GraphicsView rendering a semicircular arc gauge (opening at the bottom).
/// The arc fills from the 9-o'clock position clockwise through 12-o'clock to 3-o'clock.
///
/// Angle convention used by MAUI Graphics:
///   0° = 3 o'clock (right), angles increase clockwise (screen/compass convention).
///   Top semicircle = startAngle 180° → endAngle 360°, clockwise=true.
///
/// Overlay emoji and labels using a XAML Grid on top of this view — canvas text
/// rendering varies by platform and does not handle emoji reliably.
/// </summary>
public class RadialGauge : GraphicsView
{
    private readonly GaugeDrawable _drawable = new();

    // ── Value (0 – 1) ────────────────────────────────────────────────────────
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double), typeof(RadialGauge), 0.0,
        propertyChanged: Refresh<double>((g, v) => g._drawable.Value = v));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // ── Arc color ────────────────────────────────────────────────────────────
    public static readonly BindableProperty GaugeColorProperty = BindableProperty.Create(
        nameof(GaugeColor), typeof(Color), typeof(RadialGauge), Colors.LimeGreen,
        propertyChanged: Refresh<Color>((g, v) => g._drawable.GaugeColor = v));

    public Color GaugeColor
    {
        get => (Color)GetValue(GaugeColorProperty);
        set => SetValue(GaugeColorProperty, value);
    }

    // ── Track thickness ──────────────────────────────────────────────────────
    public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(
        nameof(StrokeWidth), typeof(float), typeof(RadialGauge), 14f,
        propertyChanged: Refresh<float>((g, v) => g._drawable.StrokeWidth = v));

    public float StrokeWidth
    {
        get => (float)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public RadialGauge()
    {
        Drawable = _drawable;
    }

    // ── Helper for property-changed refresh ──────────────────────────────────
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
        // Top-semicircle: 180° (left/9-o'clock) → 360° (right/3-o'clock), clockwise.
        private const float StartAngle = 180f;
        private const float FullSweep  = 179.9f; // avoid 0/360 ambiguity at seam

        public double Value     { get; set; }
        public Color GaugeColor { get; set; } = Colors.LimeGreen;
        public float StrokeWidth { get; set; } = 14f;

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;

            float sw  = StrokeWidth;
            float cx  = rect.Width / 2f;
            // Sit the arc center at ~68 % of height so the top arc and arc-ends all fit.
            float cy  = rect.Height * 0.68f;
            float radius = Math.Min(cx - sw, cy - sw);
            if (radius < 8f) return;

            float l = cx - radius;
            float t = cy - radius;
            float d = radius * 2f;

            // ── Background track ─────────────────────────────────────────────
            canvas.StrokeColor   = Color.FromRgba(0x88, 0x88, 0x88, 0x50);
            canvas.StrokeSize    = sw;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.DrawArc(l, t, d, d, StartAngle, StartAngle + FullSweep, true, false);

            // ── Value arc ────────────────────────────────────────────────────
            float clamped = (float)Math.Clamp(Value, 0.0, 1.0);
            if (clamped > 0.01f)
            {
                float sweep = clamped * FullSweep;
                canvas.StrokeColor   = GaugeColor;
                canvas.StrokeSize    = sw;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.DrawArc(l, t, d, d, StartAngle, StartAngle + sweep, true, false);
            }

            // ── Decorative end-cap dots at track extremes ────────────────────
            float dotR = sw * 0.5f;
            canvas.FillColor = Color.FromRgba(0x88, 0x88, 0x88, 0x50);
            // Left dot (9 o'clock on the circle)
            canvas.FillCircle(l - dotR * 0.1f, cy, dotR);
            // Right dot (3 o'clock)
            canvas.FillCircle(l + d + dotR * 0.1f, cy, dotR);
        }
    }
}
