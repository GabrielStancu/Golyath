namespace Golyath.Charts;

/// <summary>
/// Draws a horizontal bar chart — one row per muscle group.
/// Bars are drawn left-to-right, proportional to Value / MaxValue.
/// The first bar (highest volume) is highlighted in amber.
/// </summary>
public class HorizontalBarChartDrawable : IDrawable
{
    public string[] Labels { get; set; } = [];
    public float[] Values { get; set; } = [];

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values.Length == 0) return;

        float maxVal = Values.Max();
        if (maxVal <= 0) maxVal = 1;

        const float sidePad = 8f;
        const float labelW = 80f;
        const float rowH = 36f;
        const float barH = 18f;
        const float rowGap = 6f;

        for (int i = 0; i < Values.Length; i++)
        {
            float rowTop = sidePad + i * (rowH + rowGap);
            float barTop = rowTop + (rowH - barH) / 2f;
            float barAreaW = dirtyRect.Width - sidePad * 2f - labelW;
            float barW = Values[i] > 0
                ? Math.Max(6f, (Values[i] / maxVal) * barAreaW * 0.95f)
                : 0f;
            float barX = sidePad + labelW;

            // Label
            canvas.FontSize = 12f;
            canvas.FontColor = i == 0
                ? Color.FromArgb("#F5C518")
                : Color.FromArgb("#9A9A9A");
            if (i < Labels.Length)
            {
                canvas.DrawString(
                    Labels[i],
                    sidePad,
                    rowTop,
                    labelW - 4f,
                    rowH,
                    HorizontalAlignment.Right,
                    VerticalAlignment.Center);
            }

            // Background track
            canvas.FillColor = Color.FromArgb("#1E1E1E");
            canvas.FillRoundedRectangle(barX, barTop, barAreaW, barH, 5f);

            // Value bar
            if (barW > 0)
            {
                canvas.FillColor = i == 0
                    ? Color.FromArgb("#F5C518")
                    : Color.FromArgb("#3A3A3A");
                canvas.FillRoundedRectangle(barX, barTop, barW, barH, 5f);
            }

            // Percentage label on bar
            if (Values[i] > 0 && barW > 28f)
            {
                float pct = maxVal > 0 ? Values[i] / maxVal * 100f : 0f;
                canvas.FontSize = 10f;
                canvas.FontColor = i == 0
                    ? Color.FromArgb("#0A0A0A")
                    : Color.FromArgb("#888888");
                canvas.DrawString(
                    $"{pct:F0}%",
                    barX + 6f,
                    barTop,
                    barW - 10f,
                    barH,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Center);
            }
        }
    }
}
