namespace Golyath.UI.Controls;

/// <summary>
/// A GraphicsView that renders a subtle diagonal hatched line pattern.
/// Used as a decorative background overlay on the dashboard hero card.
/// </summary>
public class HatchPatternView : GraphicsView
{
    private readonly HatchDrawable _drawable = new();

    public static readonly BindableProperty LineColorProperty = BindableProperty.Create(
        nameof(LineColor), typeof(Color), typeof(HatchPatternView), Color.FromRgba(255, 215, 0, 25),
        propertyChanged: (b, _, n) => { ((HatchPatternView)b)._drawable.LineColor = (Color)n; ((HatchPatternView)b).Invalidate(); });

    public Color LineColor
    {
        get => (Color)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    public static readonly BindableProperty LineSpacingProperty = BindableProperty.Create(
        nameof(LineSpacing), typeof(float), typeof(HatchPatternView), 10f,
        propertyChanged: (b, _, n) => { ((HatchPatternView)b)._drawable.LineSpacing = (float)n; ((HatchPatternView)b).Invalidate(); });

    public float LineSpacing
    {
        get => (float)GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    public static readonly BindableProperty LineThicknessProperty = BindableProperty.Create(
        nameof(LineThickness), typeof(float), typeof(HatchPatternView), 1f,
        propertyChanged: (b, _, n) => { ((HatchPatternView)b)._drawable.LineThickness = (float)n; ((HatchPatternView)b).Invalidate(); });

    public float LineThickness
    {
        get => (float)GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    public HatchPatternView()
    {
        Drawable = _drawable;
        InputTransparent = true;
    }

    private sealed class HatchDrawable : IDrawable
    {
        public Color LineColor { get; set; } = Color.FromRgba(255, 215, 0, 25);
        public float LineSpacing { get; set; } = 10f;
        public float LineThickness { get; set; } = 1f;

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;
            canvas.StrokeColor = LineColor;
            canvas.StrokeSize = LineThickness;
            canvas.StrokeLineCap = LineCap.Butt;

            float w = rect.Width;
            float h = rect.Height;
            float spacing = LineSpacing;

            // Draw 45° diagonal lines from bottom-left to top-right
            // Lines sweep from -h to +w to cover the entire rectangle
            float total = w + h;
            for (float offset = -h; offset < total; offset += spacing)
            {
                float x1 = offset;
                float y1 = h;
                float x2 = offset + h;
                float y2 = 0;

                // Clip to rect bounds
                if (x1 < 0) { y1 += x1; x1 = 0; }
                if (x2 > w) { y2 += (x2 - w); x2 = w; }
                if (y1 > h) { x1 += (y1 - h); y1 = h; }
                if (y2 < 0) { x2 += y2; y2 = 0; }

                if (x1 <= w && x2 >= 0 && y1 >= 0 && y2 <= h)
                {
                    canvas.DrawLine(x1, y1, x2, y2);
                }
            }
        }
    }
}
