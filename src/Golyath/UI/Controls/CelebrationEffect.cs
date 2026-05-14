namespace Golyath.UI.Controls;

/// <summary>
/// Particle burst celebration effect for PR achievements.
/// Set IsActive=true to trigger — automatically resets when the animation completes.
/// Transparent when idle. Pure MAUI GraphicsView.
/// </summary>
public class CelebrationEffect : GraphicsView
{
    private readonly CelebrationDrawable _drawable = new();
    private IDispatcherTimer? _timer;
    private readonly List<Particle> _particles = new();
    private readonly Random _rng = new();

    private const int ParticleCount = 25;
    private const double LifetimeMs = 1200;

    // ── IsActive (trigger) ───────────────────────────────────────────────────
    public static readonly BindableProperty IsActiveProperty = BindableProperty.Create(
        nameof(IsActive), typeof(bool), typeof(CelebrationEffect), false,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: OnIsActiveChanged);

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    // ── Theme awareness ──────────────────────────────────────────────────────
    public static readonly BindableProperty IsDarkProperty = BindableProperty.Create(
        nameof(IsDark), typeof(bool), typeof(CelebrationEffect), false);

    public bool IsDark
    {
        get => (bool)GetValue(IsDarkProperty);
        set => SetValue(IsDarkProperty, value);
    }

    public CelebrationEffect()
    {
        Drawable = _drawable;
        BackgroundColor = Colors.Transparent;
        InputTransparent = true; // don't capture touch events
    }

    private static void OnIsActiveChanged(BindableObject b, object oldVal, object newVal)
    {
        var effect = (CelebrationEffect)b;
        if ((bool)newVal)
            effect.StartBurst();
    }

    // ── Burst initialization ─────────────────────────────────────────────────
    private void StartBurst()
    {
        _timer?.Stop();
        _particles.Clear();

        float cx = (float)(Width / 2.0);
        float cy = (float)(Height / 2.0);
        if (cx < 1) cx = 100;
        if (cy < 1) cy = 100;

        // Celebration color palette
        Color[] colors =
        [
            Color.FromArgb("#FFD700"), // Gold
            Color.FromArgb("#FFD700"),
            Color.FromArgb("#FFA500"), // Amber
            Color.FromArgb("#FFA500"),
            Color.FromArgb("#FFFFFF"), // White
            Color.FromArgb("#FFE066"), // Light gold
            Color.FromArgb("#FFCC33"), // Bright gold
        ];

        for (int i = 0; i < ParticleCount; i++)
        {
            // Random angle for radial explosion
            double angle = _rng.NextDouble() * 2.0 * Math.PI;
            // Random speed — varied for natural look
            float speed = 120f + (float)(_rng.NextDouble() * 200f);

            _particles.Add(new Particle
            {
                X = cx,
                Y = cy,
                VelocityX = (float)(Math.Cos(angle) * speed),
                VelocityY = (float)(Math.Sin(angle) * speed) - 80f, // upward bias
                Size = 2f + (float)(_rng.NextDouble() * 4f),
                Color = colors[_rng.Next(colors.Length)],
                Opacity = 1f,
                IsSquare = _rng.NextDouble() > 0.6, // ~40% squares
                Rotation = (float)(_rng.NextDouble() * 360f),
                RotationSpeed = (float)(_rng.NextDouble() * 400f - 200f),
            });
        }

        _drawable.Particles = _particles;
        _drawable.ElapsedMs = 0;

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60fps
        _timer.Tick += OnTick;
        _timer.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        const float dt = 16f / 1000f; // seconds per frame
        const float gravity = 350f;   // pixels/s²

        _drawable.ElapsedMs += 16;

        // Calculate fade based on total elapsed time
        float lifeFraction = (float)(_drawable.ElapsedMs / LifetimeMs);

        foreach (var p in _particles)
        {
            // Apply gravity
            p.VelocityY += gravity * dt;

            // Update position
            p.X += p.VelocityX * dt;
            p.Y += p.VelocityY * dt;

            // Rotate squares
            p.Rotation += p.RotationSpeed * dt;

            // Slow down horizontally (air resistance)
            p.VelocityX *= 0.985f;

            // Fade out in the second half of life
            if (lifeFraction > 0.4f)
                p.Opacity = Math.Max(0f, 1f - (lifeFraction - 0.4f) / 0.6f);
        }

        Invalidate();

        // End animation
        if (lifeFraction >= 1f)
        {
            _timer?.Stop();
            _timer = null;
            _particles.Clear();
            _drawable.Particles = _particles;
            Invalidate();

            // Reset IsActive to false so it can be triggered again
            IsActive = false;
        }
    }

    // ── Particle data ────────────────────────────────────────────────────────
    private class Particle
    {
        public float X;
        public float Y;
        public float VelocityX;
        public float VelocityY;
        public float Size;
        public Color Color = Colors.Gold;
        public float Opacity = 1f;
        public bool IsSquare;
        public float Rotation;
        public float RotationSpeed;
    }

    // ── Drawable ─────────────────────────────────────────────────────────────
    private sealed class CelebrationDrawable : IDrawable
    {
        public List<Particle> Particles { get; set; } = new();
        public double ElapsedMs { get; set; }

        public void Draw(ICanvas canvas, RectF rect)
        {
            canvas.Antialias = true;

            if (Particles.Count == 0) return;

            foreach (var p in Particles)
            {
                if (p.Opacity <= 0.01f) continue;

                canvas.FillColor = p.Color.WithAlpha(p.Opacity);

                if (p.IsSquare)
                {
                    // Draw rotated square
                    canvas.SaveState();
                    canvas.Translate(p.X, p.Y);
                    canvas.Rotate(p.Rotation);
                    float half = p.Size / 2f;
                    canvas.FillRectangle(-half, -half, p.Size, p.Size);
                    canvas.RestoreState();
                }
                else
                {
                    canvas.FillCircle(p.X, p.Y, p.Size / 2f);
                }
            }
        }
    }
}
