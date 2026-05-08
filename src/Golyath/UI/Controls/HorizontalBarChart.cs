using Golyath.Application.DTOs;

namespace Golyath.UI.Controls;

/// <summary>
/// Pure-MAUI GraphicsView that renders a horizontal bar chart for muscle-group distribution.
/// Gold bars scaled by fraction, labels on the left, set-count on the right.
/// No third-party dependencies.
/// </summary>
public class HorizontalBarChart : GraphicsView
{
    private readonly HBarDrawable _drawable = new();

    // ── Items ────────────────────────────────────────────────────────────────

    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items),
        typeof(IReadOnlyList<MuscleGroupVolume>),
        typeof(HorizontalBarChart),
        defaultValue: null,
        propertyChanged: (b, _, n) =>
        {
            var c = (HorizontalBarChart)b;
            c._drawable.Items = n as IReadOnlyList<MuscleGroupVolume> ?? [];
            c.Invalidate();
        });

    public IReadOnlyList<MuscleGroupVolume>? Items
    {
        get => (IReadOnlyList<MuscleGroupVolume>?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public HorizontalBarChart()
    {
        Drawable = _drawable;
    }

    // ── Drawable ─────────────────────────────────────────────────────────────

    private sealed class HBarDrawable : IDrawable
    {
        private const float LabelW     = 80f;
        private const float CountW     = 36f;
        private const float RowPad     = 6f;
        private const float BarH       = 18f;
        private const float RowSpacing = 30f;

        public IReadOnlyList<MuscleGroupVolume> Items { get; set; } = [];

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;

            if (Items.Count == 0)
            {
                canvas.FontColor = Color.FromArgb("#888888");
                canvas.FontSize  = 13f;
                canvas.DrawString(
                    "No data for the selected period",
                    rect.Left, rect.Top, rect.Width, rect.Height,
                    HorizontalAlignment.Center, VerticalAlignment.Center);
                return;
            }

            float barAreaW = rect.Width - LabelW - CountW - RowPad * 2f;
            var accent     = Color.FromArgb("#FFD700");
            var trackColor = Color.FromRgba(0x88, 0x88, 0x88, 0x28);
            var labelColor = Color.FromArgb("#CCCCCC");
            var dimColor   = Color.FromArgb("#888888");

            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                float rowTop = i * RowSpacing + RowPad;
                float barY   = rowTop + (RowSpacing - BarH) / 2f;

                // Muscle label
                canvas.FontColor = labelColor;
                canvas.FontSize  = 12f;
                canvas.DrawString(
                    item.MuscleGroup,
                    RowPad, rowTop,
                    LabelW - RowPad, RowSpacing,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Center);

                float barX = LabelW;

                // Background track
                canvas.FillColor = trackColor;
                canvas.FillRoundedRectangle(barX, barY, barAreaW, BarH, BarH / 2f);

                // Filled portion
                float fillW = (float)(item.Fraction * barAreaW);
                if (fillW > 2f)
                {
                    canvas.FillColor = accent;
                    canvas.FillRoundedRectangle(barX, barY, fillW, BarH, BarH / 2f);
                }

                // Set count
                canvas.FontColor = dimColor;
                canvas.FontSize  = 11f;
                canvas.DrawString(
                    item.SetCount.ToString(),
                    barX + barAreaW + RowPad, rowTop,
                    CountW, RowSpacing,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Center);
            }
        }
    }
}
