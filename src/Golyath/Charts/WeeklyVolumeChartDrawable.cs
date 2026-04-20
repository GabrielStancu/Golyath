namespace Golyath.Charts;

/// <summary>
/// Draws a flat bar chart for weekly workout volume.
/// Fill the Values and Labels arrays, set HighlightIndex for "today", then
/// assign a new instance to the binding to trigger GraphicsView redraw.
/// </summary>
public class WeeklyVolumeChartDrawable : IDrawable
{
    public float[] Values { get; set; } = [];
    public string[] Labels { get; set; } = [];
    /// <summary>0-based index of today's bar (highlighted in amber).</summary>
    public int HighlightIndex { get; set; } = -1;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values.Length == 0) return;

        float maxVal = Values.Max();
        if (maxVal <= 0) maxVal = 1;

        const float sidePad = 8f;
        const float topPad = 10f;
        const float labelH = 22f;
        const float gapRatio = 0.38f;

        float slotW = (dirtyRect.Width - sidePad * 2f) / Values.Length;
        float barW = slotW * (1f - gapRatio);
        float barX0 = sidePad + slotW * (gapRatio / 2f);
        float chartBottom = dirtyRect.Height - labelH;
        float chartH = chartBottom - topPad;

        // Subtle grid line at top
        canvas.StrokeColor = Color.FromArgb("#1E1E1E");
        canvas.StrokeSize = 1f;
        canvas.DrawLine(sidePad, topPad + chartH * 0.15f, dirtyRect.Width - sidePad, topPad + chartH * 0.15f);
        canvas.DrawLine(sidePad, topPad + chartH * 0.5f, dirtyRect.Width - sidePad, topPad + chartH * 0.5f);
        canvas.DrawLine(sidePad, topPad + chartH * 0.85f, dirtyRect.Width - sidePad, topPad + chartH * 0.85f);

        for (int i = 0; i < Values.Length; i++)
        {
            bool isToday = i == HighlightIndex;
            float barH = Values[i] > 0
                ? Math.Max(6f, (Values[i] / maxVal) * chartH * 0.88f)
                : 0f;

            float x = barX0 + i * slotW;
            float barTop = chartBottom - barH;

            if (barH > 0)
            {
                canvas.FillColor = isToday
                    ? Color.FromArgb("#F5C518")
                    : Color.FromArgb("#2E2E2E");
                canvas.FillRoundedRectangle(x, barTop, barW, barH, 5f);

                // Accent top cap for non-today bars
                if (!isToday)
                {
                    canvas.FillColor = Color.FromArgb("#3A3A3A");
                    canvas.FillRoundedRectangle(x, barTop, barW, 4f, 5f);
                }
            }
            else
            {
                // Rest day: tiny indicator at bottom
                canvas.FillColor = Color.FromArgb("#1E1E1E");
                canvas.FillRoundedRectangle(x, chartBottom - 3f, barW, 3f, 2f);
            }

            // Today dot above bar
            if (isToday && barH > 0)
            {
                canvas.FillColor = Color.FromArgb("#F5C518");
                canvas.FillCircle(x + barW / 2f, barTop - 8f, 3.5f);
            }

            // Label
            canvas.FontSize = 11f;
            canvas.FontColor = isToday
                ? Color.FromArgb("#F5C518")
                : Color.FromArgb("#5A5A5A");
            if (i < Labels.Length)
            {
                canvas.DrawString(
                    Labels[i],
                    x - slotW * gapRatio / 2f,
                    chartBottom + 3f,
                    slotW,
                    labelH - 3f,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Top);
            }
        }
    }
}
