namespace Golyath.UI.Controls;

/// <summary>
/// Inline bar chart that renders completed-set volumes as gold bars, growing left to right.
/// The most recent bar is bright gold; older bars are dimmed.
/// </summary>
public class EffortCurveChart : GraphicsView
{
    public static readonly BindableProperty PointsProperty = BindableProperty.Create(
        nameof(Points), typeof(double[]), typeof(EffortCurveChart), Array.Empty<double>(),
        propertyChanged: (b, _, _) => ((EffortCurveChart)b).InvalidateChart());

    public double[] Points
    {
        get => (double[])GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public EffortCurveChart()
    {
        Drawable = new EffortDrawable(this);
        HeightRequest = 48;
        WidthRequest  = 90;
    }

    private void InvalidateChart() => this.Invalidate();

    private sealed class EffortDrawable : IDrawable
    {
        private readonly EffortCurveChart _owner;
        public EffortDrawable(EffortCurveChart owner) => _owner = owner;

        public void Draw(ICanvas canvas, RectF rect)
        {
            var points = _owner.Points;
            if (points is null || points.Length == 0) return;

            canvas.Antialias = true;

            var brightGold = Color.FromArgb("#FFD700");
            var dimGold    = Color.FromArgb("#5C4A00");

            int   count = points.Length;
            float gap   = 3f;
            float barW  = (rect.Width - gap * (count - 1)) / count;
            float minH  = 4f;
            double max  = points.Max();
            if (max < 1) max = 1;

            for (int i = 0; i < count; i++)
            {
                float barH = (float)(points[i] / max * (rect.Height - minH)) + minH;
                float x    = i * (barW + gap);
                float y    = rect.Bottom - barH;

                canvas.FillColor = i == count - 1 ? brightGold : dimGold;
                canvas.FillRoundedRectangle(x, y, barW, barH, 2f);
            }
        }
    }
}
