namespace Golyath.UI.Controls;

/// <summary>
/// Platform-agnostic horizontal progress fill bar drawn via GraphicsView.
/// Avoids the native ProgressBar outline/shadow on Windows.
/// </summary>
public class ProgressFillView : GraphicsView
{
    public static readonly BindableProperty ProgressProperty = BindableProperty.Create(
        nameof(Progress), typeof(double), typeof(ProgressFillView), 0.0,
        propertyChanged: (b, _, _) => ((ProgressFillView)b).Invalidate());

    public static readonly BindableProperty FillColorProperty = BindableProperty.Create(
        nameof(FillColor), typeof(Color), typeof(ProgressFillView), Color.FromArgb("#FFD700"),
        propertyChanged: (b, _, _) => ((ProgressFillView)b).Invalidate());

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor), typeof(Color), typeof(ProgressFillView), Color.FromArgb("#2A2A2A"),
        propertyChanged: (b, _, _) => ((ProgressFillView)b).Invalidate());

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public Color FillColor
    {
        get => (Color)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public ProgressFillView()
    {
        Drawable = new ProgressDrawable(this);
        HeightRequest = 4;
    }

    private sealed class ProgressDrawable : IDrawable
    {
        private readonly ProgressFillView _owner;
        public ProgressDrawable(ProgressFillView owner) => _owner = owner;

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;
            float radius = rect.Height / 2f;

            // Track
            canvas.FillColor = _owner.TrackColor;
            canvas.FillRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, radius);

            // Fill
            double fraction = Math.Clamp(_owner.Progress, 0.0, 1.0);
            if (fraction > 0)
            {
                float fillW = (float)(rect.Width * fraction);
                canvas.FillColor = _owner.FillColor;
                canvas.FillRoundedRectangle(rect.X, rect.Y, fillW, rect.Height, radius);
            }
        }
    }
}
