namespace Golyath.UI.Controls;

/// <summary>
/// Draws the bottom nav bar background with a smooth concave notch
/// at the top-centre to accommodate an overflowing FAB button.
/// </summary>
public class NotchedNavBarDrawable : IDrawable
{
    public Color FillColor { get; set; } = Colors.White;

    public void Draw(ICanvas canvas, RectF dirty)
    {
        float w  = dirty.Width;
        float h  = dirty.Height;
        float cx = w / 2f;

        // Notch dimensions — tuned to a 64 px FAB that sits ~30 px into the bar.
        // The bezier curve mirrors the FAB's lower arc so the two shapes feel connected.
        const float notchHalfW = 38f;  // half-width of the notch opening at bar top
        const float notchDepth = 32f;  // how far the notch descends into the bar
        const float blend      = 14f;  // horizontal blend distance for smooth entry/exit

        var path = new PathF();

        // ── Top edge: flat left section ───────────────────────────────────
        path.MoveTo(0, 0);
        path.LineTo(cx - notchHalfW - blend, 0);

        // ── Left bezier: curves smoothly down into the notch ─────────────
        path.CurveTo(
            cx - notchHalfW, 0f,           // CP1 — pulls curve tangent at entry
            cx - notchHalfW, notchDepth,   // CP2 — locks depth at the trough
            cx,              notchDepth    // end — bottom of notch (centre)
        );

        // ── Right bezier: curves smoothly back up out of the notch ───────
        path.CurveTo(
            cx + notchHalfW, notchDepth,   // CP1 — symmetric to left
            cx + notchHalfW, 0f,           // CP2
            cx + notchHalfW + blend, 0f    // end — back to bar top
        );

        // ── Top edge: flat right section ─────────────────────────────────
        path.LineTo(w, 0);

        // ── Rectangle remainder of bar ────────────────────────────────────
        path.LineTo(w, h);
        path.LineTo(0, h);
        path.Close();

        // Subtle top shadow that follows the notch shape
        canvas.SetShadow(new SizeF(0, -3), 8, Colors.Black.WithAlpha(0.09f));
        canvas.FillColor = FillColor;
        canvas.FillPath(path);
        // Reset shadow by drawing transparent to clear state
        canvas.SetShadow(new SizeF(0, 0), 0, Colors.Transparent);
    }
}
