namespace Golyath.Charts;

/// <summary>
/// Bar chart showing one integer value per labelled slot (e.g. sessions per week).
/// Identical structure to <see cref="WeeklyVolumeChartDrawable"/> but accepts int values
/// and renders count labels above each non-zero bar.
/// </summary>
public class FrequencyChartDrawable : IDrawable
{
    public int[] Values { get; set; } = [];
    public string[] Labels { get; set; } = [];
    public int HighlightIndex { get; set; } = -1;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values.Length == 0) return;

        int maxVal = Values.Max();
        if (maxVal <= 0) maxVal = 1;

        const float sidePad = 8f;
        const float topPad = 18f; // room for count label above bar
        const float labelH = 22f;
        const float gapRatio = 0.38f;

        float slotW = (dirtyRect.Width - sidePad * 2f) / Values.Length;
        float barW = slotW * (1f - gapRatio);
        float barX0 = sidePad + slotW * (gapRatio / 2f);
        float chartBottom = dirtyRect.Height - labelH;
        float chartH = chartBottom - topPad;

        // Grid lines
        canvas.StrokeColor = Color.FromArgb("#1E1E1E");
        canvas.StrokeSize = 1f;
        canvas.DrawLine(sidePad, topPad + chartH * 0.5f,  dirtyRect.Width - sidePad, topPad + chartH * 0.5f);
        canvas.DrawLine(sidePad, topPad + chartH * 0.85f, dirtyRect.Width - sidePad, topPad + chartH * 0.85f);

        for (int i = 0; i < Values.Length; i++)
        {
            bool highlight = i == HighlightIndex;
            float barH = Values[i] > 0
                ? Math.Max(6f, (float)Values[i] / maxVal * chartH * 0.88f)
                : 0f;

            float x = barX0 + i * slotW;
            float barTop = chartBottom - barH;

            if (barH > 0)
            {
                canvas.FillColor = highlight
                    ? Color.FromArgb("#F5C518")
                    : Color.FromArgb("#2E2E2E");
                canvas.FillRoundedRectangle(x, barTop, barW, barH, 5f);

                if (!highlight)
                {
                    canvas.FillColor = Color.FromArgb("#3A3A3A");
                    canvas.FillRoundedRectangle(x, barTop, barW, 4f, 5f);
                }

                // Count above bar
                canvas.FontSize = 10f;
                canvas.FontColor = highlight
                    ? Color.FromArgb("#F5C518")
                    : Color.FromArgb("#5A5A5A");
                canvas.DrawString(
                    Values[i].ToString(),
                    x - slotW * gapRatio / 2f,
                    barTop - 16f,
                    slotW,
                    16f,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Top);
            }
            else
            {
                canvas.FillColor = Color.FromArgb("#1E1E1E");
                canvas.FillRoundedRectangle(x, chartBottom - 3f, barW, 3f, 2f);
            }

            // Day/week label
            canvas.FontSize = 11f;
            canvas.FontColor = highlight
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
