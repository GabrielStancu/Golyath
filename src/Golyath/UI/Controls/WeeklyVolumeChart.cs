namespace Golyath.UI.Controls;

/// <summary>
/// Simple 4-bar volume chart rendered with ICanvas.
/// Gold bars proportional to their max value, with week labels below.
/// </summary>
public class WeeklyVolumeChart : GraphicsView
{
    private readonly ChartDrawable _drawable = new();

    public static readonly BindableProperty VolumesProperty = BindableProperty.Create(
        nameof(Volumes), typeof(double[]), typeof(WeeklyVolumeChart),
        new double[] { 0, 0, 0, 0 },
        propertyChanged: (b, _, n) =>
        {
            var v = (WeeklyVolumeChart)b;
            v._drawable.Volumes = (double[])n;
            v.Invalidate();
        });

    public static readonly BindableProperty LabelsProperty = BindableProperty.Create(
        nameof(Labels), typeof(string[]), typeof(WeeklyVolumeChart),
        new string[] { "W1", "W2", "W3", "W4" },
        propertyChanged: (b, _, n) =>
        {
            var v = (WeeklyVolumeChart)b;
            v._drawable.Labels = (string[])n;
            v.Invalidate();
        });

    public double[] Volumes
    {
        get => (double[])GetValue(VolumesProperty);
        set => SetValue(VolumesProperty, value);
    }

    public string[] Labels
    {
        get => (string[])GetValue(LabelsProperty);
        set => SetValue(LabelsProperty, value);
    }

    public WeeklyVolumeChart()
    {
        Drawable = _drawable;
        InputTransparent = true;
        HeightRequest = 80;
        HorizontalOptions = LayoutOptions.Fill;
    }

    private sealed class ChartDrawable : IDrawable
    {
        public double[] Volumes { get; set; } = [0, 0, 0, 0];
        public string[] Labels { get; set; } = ["W1", "W2", "W3", "W4"];

        private static readonly Color GoldDim = Color.FromArgb("#9A7B00");
        private static readonly Color GoldBright = Color.FromArgb("#FFD700");
        private static readonly Color LabelColor = Color.FromArgb("#888888");

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;

            int count = Math.Min(Volumes.Length, 4);
            double max = Volumes.Take(count).Max();
            if (max <= 0) max = 1;

            float labelHeight = 18f;
            float chartHeight = rect.Height - labelHeight;
            float barAreaWidth = rect.Width / count;
            float barPad = barAreaWidth * 0.22f;
            float barWidth = barAreaWidth - barPad * 2;
            float minBarHeight = 4f;

            for (int i = 0; i < count; i++)
            {
                float x = rect.Left + i * barAreaWidth + barPad;
                double ratio = Volumes[i] / max;
                float barH = (float)(ratio * chartHeight);
                if (barH < minBarHeight && Volumes[i] > 0) barH = minBarHeight;

                float barY = rect.Top + chartHeight - barH;

                // Last bar (current week) is bright gold
                bool isCurrent = i == count - 1;
                canvas.FillColor = isCurrent ? GoldBright : GoldDim;
                canvas.FillRoundedRectangle(x, barY, barWidth, barH, 3);

                // Label below bar
                string label = i < Labels.Length ? Labels[i] : $"W{i + 1}";
                canvas.FontColor = isCurrent ? GoldBright : LabelColor;
                canvas.FontSize = 9;
                canvas.DrawString(label, x, rect.Top + chartHeight + 2, barWidth, labelHeight,
                    HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }
    }
}
