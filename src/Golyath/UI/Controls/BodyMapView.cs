namespace Golyath.UI.Controls;

/// <summary>
/// A simplified blocky front/back human figure. Each body region is coloured with
/// a gold gradient whose opacity reflects that muscle group's training volume fraction
/// (0 = dim grey, 1 = full gold). Toggle IsFrontView to flip between orientations.
/// </summary>
public class BodyMapView : GraphicsView
{
    private readonly BodyMapDrawable _drawable = new();

    // ── MuscleWeights ────────────────────────────────────────────────────────

    public static readonly BindableProperty MuscleWeightsProperty = BindableProperty.Create(
        nameof(MuscleWeights),
        typeof(IReadOnlyDictionary<string, double>),
        typeof(BodyMapView),
        null,
        propertyChanged: (b, _, n) =>
        {
            var v = (BodyMapView)b;
            v._drawable.MuscleWeights = n as IReadOnlyDictionary<string, double>;
            v.Invalidate();
        });

    public IReadOnlyDictionary<string, double>? MuscleWeights
    {
        get => (IReadOnlyDictionary<string, double>?)GetValue(MuscleWeightsProperty);
        set => SetValue(MuscleWeightsProperty, value);
    }

    // ── IsFrontView ──────────────────────────────────────────────────────────

    public static readonly BindableProperty IsFrontViewProperty = BindableProperty.Create(
        nameof(IsFrontView), typeof(bool), typeof(BodyMapView), true,
        propertyChanged: (b, _, n) =>
        {
            var v = (BodyMapView)b;
            v._drawable.IsFrontView = (bool)n;
            v.Invalidate();
        });

    public bool IsFrontView
    {
        get => (bool)GetValue(IsFrontViewProperty);
        set => SetValue(IsFrontViewProperty, value);
    }

    public BodyMapView()
    {
        Drawable = _drawable;
        InputTransparent = true;
        HeightRequest = 220;
        WidthRequest = 110;
        HorizontalOptions = LayoutOptions.Center;
    }

    // ─────────────────────────────────────────────────────────────────────────

    private sealed class BodyMapDrawable : IDrawable
    {
        // Reference canvas 110 × 220
        private const float RefW = 110f;
        private const float RefH = 220f;

        private static readonly Color GoldFull = Color.FromArgb("#FFD700");
        private static readonly Color Muted     = Color.FromArgb("#2C2C2C");
        private static readonly Color MutedBdr  = Color.FromArgb("#3A3A3A");
        private static readonly Color Neutral   = Color.FromArgb("#242424");

        public IReadOnlyDictionary<string, double>? MuscleWeights { get; set; }
        public bool IsFrontView { get; set; } = true;

        private double W(string key) => MuscleWeights?.GetValueOrDefault(key, 0.0) ?? 0.0;

        private Color MuscleColor(double fraction)
        {
            if (fraction <= 0.005) return Muted;
            return GoldFull.WithAlpha((float)Math.Max(0.18, fraction));
        }

        public void Draw(ICanvas canvas, RectF dirty)
        {
            canvas.Antialias = true;

            float sx = dirty.Width  / RefW;
            float sy = dirty.Height / RefH;
            canvas.SaveState();
            canvas.Scale(sx, sy);

            float cx = RefW / 2f;   // 55

            double upperTorso = IsFrontView ? W("Chest")   : W("Back");
            double lowerTorso = IsFrontView ? W("Core")    : W("Legs");
            double shoulders  = W("Shoulders");
            double arms       = W("Arms");
            double legs       = W("Legs");

            // ── HEAD ────────────────────────────────────────────────────
            Fill(canvas, Neutral);
            canvas.FillEllipse(cx - 9f, 2f, 18f, 20f);

            // ── NECK ────────────────────────────────────────────────────
            Fill(canvas, Neutral);
            canvas.FillRoundedRectangle(cx - 4f, 21f, 8f, 8f, 2f);

            // ── SHOULDER CAPS ───────────────────────────────────────────
            Fill(canvas, MuscleColor(shoulders));
            canvas.FillRoundedRectangle(cx - 35f, 25f, 17f, 19f, 5f);
            canvas.FillRoundedRectangle(cx + 18f, 25f, 17f, 19f, 5f);

            // ── UPPER TORSO (Chest front / Back rear) ───────────────────
            Fill(canvas, MuscleColor(upperTorso));
            canvas.FillRoundedRectangle(cx - 19f, 29f, 38f, 46f, 6f);

            // ── LOWER TORSO (Core front / Glutes rear) ──────────────────
            Fill(canvas, MuscleColor(lowerTorso));
            canvas.FillRoundedRectangle(cx - 16f, 77f, 32f, 26f, 5f);

            // ── UPPER ARMS (Biceps/Triceps) ──────────────────────────────
            Fill(canvas, MuscleColor(arms));
            canvas.FillRoundedRectangle(cx - 37f, 31f, 16f, 42f, 6f);
            canvas.FillRoundedRectangle(cx + 21f, 31f, 16f, 42f, 6f);

            // ── FOREARMS ─────────────────────────────────────────────────
            Fill(canvas, MuscleColor(arms));
            canvas.FillRoundedRectangle(cx - 35f, 75f, 14f, 28f, 5f);
            canvas.FillRoundedRectangle(cx + 21f, 75f, 14f, 28f, 5f);

            // ── UPPER LEGS ───────────────────────────────────────────────
            Fill(canvas, MuscleColor(legs));
            canvas.FillRoundedRectangle(cx - 19f, 105f, 17f, 58f, 5f);
            canvas.FillRoundedRectangle(cx + 2f,  105f, 17f, 58f, 5f);

            // ── LOWER LEGS ───────────────────────────────────────────────
            Fill(canvas, MuscleColor(legs));
            canvas.FillRoundedRectangle(cx - 17f, 165f, 13f, 48f, 4f);
            canvas.FillRoundedRectangle(cx + 4f,  165f, 13f, 48f, 4f);

            // ── FEET ─────────────────────────────────────────────────────
            Fill(canvas, Neutral);
            canvas.FillRoundedRectangle(cx - 19f, 215f, 16f, 9f, 3f);
            canvas.FillRoundedRectangle(cx + 3f,  215f, 16f, 9f, 3f);

            canvas.RestoreState();
        }

        private static void Fill(ICanvas canvas, Color c) => canvas.FillColor = c;
    }
}
