namespace Golyath.UI.Controls;

/// <summary>
/// A GraphicsView that renders an anatomical human body silhouette (front view)
/// with separated muscle groups drawn using bezier curves. Selected muscles glow gold.
/// </summary>
public class BodyMapView : GraphicsView
{
    private readonly BodyMapDrawable _drawable = new();

    public static readonly BindableProperty SelectedMusclesProperty = BindableProperty.Create(
        nameof(SelectedMuscles), typeof(IReadOnlySet<string>), typeof(BodyMapView),
        new HashSet<string>() as IReadOnlySet<string>,
        propertyChanged: (b, _, n) =>
        {
            var view = (BodyMapView)b;
            view._drawable.SelectedMuscles = (IReadOnlySet<string>)n;
            view.Invalidate();
        });

    public IReadOnlySet<string> SelectedMuscles
    {
        get => (IReadOnlySet<string>)GetValue(SelectedMusclesProperty);
        set => SetValue(SelectedMusclesProperty, value);
    }

    public BodyMapView()
    {
        Drawable = _drawable;
        InputTransparent = true;
        HeightRequest = 280;
        WidthRequest = 140;
        HorizontalOptions = LayoutOptions.Center;
    }

    private sealed class BodyMapDrawable : IDrawable
    {
        public IReadOnlySet<string> SelectedMuscles { get; set; } = new HashSet<string>();

        private static readonly Color GoldActive = Color.FromArgb("#FFD700");
        private static readonly Color Muted = Color.FromArgb("#2A2A2A");
        private static readonly Color MutedBorder = Color.FromArgb("#3A3A3A");

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.Antialias = true;

            // Reference canvas: 140 × 280; scale uniformly to actual size
            float sx = dirtyRect.Width / 140f;
            float sy = dirtyRect.Height / 280f;
            canvas.SaveState();
            canvas.Scale(sx, sy);

            const float cx = 70f;

            bool shoulders = SelectedMuscles.Contains("Shoulders");
            bool chest = SelectedMuscles.Contains("Chest");
            bool back = SelectedMuscles.Contains("Back");
            bool biceps = SelectedMuscles.Contains("Biceps");
            bool triceps = SelectedMuscles.Contains("Triceps");
            bool core = SelectedMuscles.Contains("Core");
            bool legs = SelectedMuscles.Contains("Legs");

            // ─── HEAD ──────────────────────────────────────────────
            Ellipse(canvas, cx - 10, 3, 20, 24, false);

            // ─── NECK ──────────────────────────────────────────────
            Shape(canvas, false, p =>
            {
                p.MoveTo(cx - 6, 25);
                p.CurveTo(cx - 6, 28, cx - 7, 33, cx - 7, 37);
                p.LineTo(cx + 7, 37);
                p.CurveTo(cx + 7, 33, cx + 6, 28, cx + 6, 25);
                p.Close();
            });

            // ─── LEFT TRAP ── sloping from neck to shoulder ────────
            Shape(canvas, shoulders, p =>
            {
                p.MoveTo(cx - 7, 32);
                p.CurveTo(cx - 16, 30, cx - 26, 34, cx - 30, 40);
                p.LineTo(cx - 20, 46);
                p.LineTo(cx - 8, 39);
                p.Close();
            });
            // ─── RIGHT TRAP ────────────────────────────────────────
            Shape(canvas, shoulders, p =>
            {
                p.MoveTo(cx + 7, 32);
                p.CurveTo(cx + 16, 30, cx + 26, 34, cx + 30, 40);
                p.LineTo(cx + 20, 46);
                p.LineTo(cx + 8, 39);
                p.Close();
            });

            // ─── LEFT DELT ── rounded shoulder cap ─────────────────
            Shape(canvas, shoulders, p =>
            {
                p.MoveTo(cx - 30, 40);
                p.CurveTo(cx - 38, 42, cx - 44, 52, cx - 40, 60);
                p.CurveTo(cx - 36, 64, cx - 30, 62, cx - 26, 58);
                p.LineTo(cx - 20, 46);
                p.Close();
            });
            // ─── RIGHT DELT ────────────────────────────────────────
            Shape(canvas, shoulders, p =>
            {
                p.MoveTo(cx + 30, 40);
                p.CurveTo(cx + 38, 42, cx + 44, 52, cx + 40, 60);
                p.CurveTo(cx + 36, 64, cx + 30, 62, cx + 26, 58);
                p.LineTo(cx + 20, 46);
                p.Close();
            });

            // ─── LEFT PEC ──────────────────────────────────────────
            Shape(canvas, chest, p =>
            {
                p.MoveTo(cx - 2, 42);
                p.CurveTo(cx - 8, 44, cx - 20, 48, cx - 24, 54);
                p.CurveTo(cx - 28, 60, cx - 26, 68, cx - 18, 70);
                p.CurveTo(cx - 10, 72, cx - 4, 66, cx - 2, 58);
                p.Close();
            });
            // ─── RIGHT PEC ─────────────────────────────────────────
            Shape(canvas, chest, p =>
            {
                p.MoveTo(cx + 2, 42);
                p.CurveTo(cx + 8, 44, cx + 20, 48, cx + 24, 54);
                p.CurveTo(cx + 28, 60, cx + 26, 68, cx + 18, 70);
                p.CurveTo(cx + 10, 72, cx + 4, 66, cx + 2, 58);
                p.Close();
            });

            // ─── LEFT LAT ── creates V-taper ───────────────────────
            Shape(canvas, back, p =>
            {
                p.MoveTo(cx - 24, 56);
                p.CurveTo(cx - 28, 62, cx - 30, 74, cx - 26, 86);
                p.CurveTo(cx - 24, 90, cx - 20, 90, cx - 18, 88);
                p.LineTo(cx - 18, 58);
                p.Close();
            });
            // ─── RIGHT LAT ────────────────────────────────────────
            Shape(canvas, back, p =>
            {
                p.MoveTo(cx + 24, 56);
                p.CurveTo(cx + 28, 62, cx + 30, 74, cx + 26, 86);
                p.CurveTo(cx + 24, 90, cx + 20, 90, cx + 18, 88);
                p.LineTo(cx + 18, 58);
                p.Close();
            });

            // ─── LEFT BICEP (inner arm) ────────────────────────────
            Shape(canvas, biceps, p =>
            {
                p.MoveTo(cx - 26, 58);
                p.CurveTo(cx - 24, 64, cx - 22, 76, cx - 22, 86);
                p.CurveTo(cx - 22, 92, cx - 24, 96, cx - 26, 98);
                p.LineTo(cx - 32, 94);
                p.CurveTo(cx - 34, 86, cx - 34, 72, cx - 32, 62);
                p.Close();
            });
            // ─── RIGHT BICEP ───────────────────────────────────────
            Shape(canvas, biceps, p =>
            {
                p.MoveTo(cx + 26, 58);
                p.CurveTo(cx + 24, 64, cx + 22, 76, cx + 22, 86);
                p.CurveTo(cx + 22, 92, cx + 24, 96, cx + 26, 98);
                p.LineTo(cx + 32, 94);
                p.CurveTo(cx + 34, 86, cx + 34, 72, cx + 32, 62);
                p.Close();
            });

            // ─── LEFT TRICEP (outer arm) ───────────────────────────
            Shape(canvas, triceps, p =>
            {
                p.MoveTo(cx - 40, 60);
                p.CurveTo(cx - 42, 66, cx - 44, 78, cx - 42, 88);
                p.CurveTo(cx - 40, 94, cx - 38, 96, cx - 36, 98);
                p.LineTo(cx - 34, 94);
                p.CurveTo(cx - 36, 86, cx - 38, 72, cx - 36, 62);
                p.Close();
            });
            // ─── RIGHT TRICEP ──────────────────────────────────────
            Shape(canvas, triceps, p =>
            {
                p.MoveTo(cx + 40, 60);
                p.CurveTo(cx + 42, 66, cx + 44, 78, cx + 42, 88);
                p.CurveTo(cx + 40, 94, cx + 38, 96, cx + 36, 98);
                p.LineTo(cx + 34, 94);
                p.CurveTo(cx + 36, 86, cx + 38, 72, cx + 36, 62);
                p.Close();
            });

            // ─── LEFT FOREARM ──────────────────────────────────────
            Shape(canvas, false, p =>
            {
                p.MoveTo(cx - 36, 98);
                p.CurveTo(cx - 40, 104, cx - 44, 118, cx - 42, 132);
                p.CurveTo(cx - 40, 138, cx - 36, 140, cx - 34, 138);
                p.LineTo(cx - 28, 134);
                p.CurveTo(cx - 24, 120, cx - 24, 108, cx - 26, 98);
                p.Close();
            });
            // ─── RIGHT FOREARM ─────────────────────────────────────
            Shape(canvas, false, p =>
            {
                p.MoveTo(cx + 36, 98);
                p.CurveTo(cx + 40, 104, cx + 44, 118, cx + 42, 132);
                p.CurveTo(cx + 40, 138, cx + 36, 140, cx + 34, 138);
                p.LineTo(cx + 28, 134);
                p.CurveTo(cx + 24, 120, cx + 24, 108, cx + 26, 98);
                p.Close();
            });

            // ─── HANDS ─────────────────────────────────────────────
            Ellipse(canvas, cx - 42, 136, 12, 14, false);
            Ellipse(canvas, cx + 30, 136, 12, 14, false);

            // ─── ABS (6-pack grid) ─────────────────────────────────
            float aL = cx - 14, aR = cx + 2, aW = 12;
            RRect(canvas, aL, 62, aW, 10, 2, core);
            RRect(canvas, aR, 62, aW, 10, 2, core);
            RRect(canvas, aL, 74, aW, 10, 2, core);
            RRect(canvas, aR, 74, aW, 10, 2, core);
            RRect(canvas, aL + 1, 86, aW - 1, 10, 2, core);
            RRect(canvas, aR, 86, aW - 1, 10, 2, core);

            // ─── LEFT OBLIQUE ──────────────────────────────────────
            Shape(canvas, core, p =>
            {
                p.MoveTo(cx - 16, 62);
                p.CurveTo(cx - 22, 68, cx - 26, 84, cx - 22, 98);
                p.LineTo(cx - 16, 98);
                p.CurveTo(cx - 18, 84, cx - 16, 68, cx - 14, 62);
                p.Close();
            });
            // ─── RIGHT OBLIQUE ─────────────────────────────────────
            Shape(canvas, core, p =>
            {
                p.MoveTo(cx + 16, 62);
                p.CurveTo(cx + 22, 68, cx + 26, 84, cx + 22, 98);
                p.LineTo(cx + 16, 98);
                p.CurveTo(cx + 18, 84, cx + 16, 68, cx + 14, 62);
                p.Close();
            });

            // ─── LEFT HIP / PELVIS ─────────────────────────────────
            Shape(canvas, legs, p =>
            {
                p.MoveTo(cx - 22, 96);
                p.CurveTo(cx - 28, 102, cx - 34, 114, cx - 12, 122);
                p.LineTo(cx - 2, 122);
                p.LineTo(cx - 2, 96);
                p.Close();
            });
            // ─── RIGHT HIP ────────────────────────────────────────
            Shape(canvas, legs, p =>
            {
                p.MoveTo(cx + 22, 96);
                p.CurveTo(cx + 28, 102, cx + 34, 114, cx + 12, 122);
                p.LineTo(cx + 2, 122);
                p.LineTo(cx + 2, 96);
                p.Close();
            });

            // ─── LEFT QUAD ─────────────────────────────────────────
            Shape(canvas, legs, p =>
            {
                p.MoveTo(cx - 12, 122);
                p.CurveTo(cx - 6, 130, cx - 4, 154, cx - 6, 178);
                p.CurveTo(cx - 8, 184, cx - 16, 186, cx - 20, 182);
                p.CurveTo(cx - 30, 174, cx - 36, 146, cx - 32, 122);
                p.Close();
            });
            // ─── RIGHT QUAD ────────────────────────────────────────
            Shape(canvas, legs, p =>
            {
                p.MoveTo(cx + 12, 122);
                p.CurveTo(cx + 6, 130, cx + 4, 154, cx + 6, 178);
                p.CurveTo(cx + 8, 184, cx + 16, 186, cx + 20, 182);
                p.CurveTo(cx + 30, 174, cx + 36, 146, cx + 32, 122);
                p.Close();
            });

            // ─── KNEES ─────────────────────────────────────────────
            Ellipse(canvas, cx - 20, 182, 16, 12, false);
            Ellipse(canvas, cx + 4, 182, 16, 12, false);

            // ─── LEFT CALF ── diamond gastrocnemius ─────────────────
            Shape(canvas, legs, p =>
            {
                p.MoveTo(cx - 18, 194);
                p.CurveTo(cx - 24, 200, cx - 28, 216, cx - 24, 238);
                p.CurveTo(cx - 22, 246, cx - 16, 250, cx - 10, 246);
                p.CurveTo(cx - 6, 244, cx - 4, 230, cx - 6, 216);
                p.CurveTo(cx - 8, 204, cx - 12, 196, cx - 14, 194);
                p.Close();
            });
            // ─── RIGHT CALF ────────────────────────────────────────
            Shape(canvas, legs, p =>
            {
                p.MoveTo(cx + 18, 194);
                p.CurveTo(cx + 24, 200, cx + 28, 216, cx + 24, 238);
                p.CurveTo(cx + 22, 246, cx + 16, 250, cx + 10, 246);
                p.CurveTo(cx + 6, 244, cx + 4, 230, cx + 6, 216);
                p.CurveTo(cx + 8, 204, cx + 12, 196, cx + 14, 194);
                p.Close();
            });

            // ─── LEFT FOOT ─────────────────────────────────────────
            Shape(canvas, false, p =>
            {
                p.MoveTo(cx - 22, 250);
                p.CurveTo(cx - 26, 254, cx - 28, 266, cx - 24, 272);
                p.LineTo(cx - 6, 272);
                p.CurveTo(cx - 4, 266, cx - 6, 256, cx - 8, 250);
                p.Close();
            });
            // ─── RIGHT FOOT ────────────────────────────────────────
            Shape(canvas, false, p =>
            {
                p.MoveTo(cx + 22, 250);
                p.CurveTo(cx + 26, 254, cx + 28, 266, cx + 24, 272);
                p.LineTo(cx + 6, 272);
                p.CurveTo(cx + 4, 266, cx + 6, 256, cx + 8, 250);
                p.Close();
            });

            canvas.RestoreState();
        }

        private void Shape(ICanvas canvas, bool active, Action<PathF> build)
        {
            var path = new PathF();
            build(path);
            canvas.FillColor = active ? GoldActive : Muted;
            canvas.FillPath(path);
            if (!active)
            {
                canvas.StrokeColor = MutedBorder;
                canvas.StrokeSize = 0.5f;
                canvas.DrawPath(path);
            }
        }

        private void Ellipse(ICanvas canvas, float x, float y, float w, float h, bool active)
        {
            canvas.FillColor = active ? GoldActive : Muted;
            canvas.FillEllipse(x, y, w, h);
            if (!active)
            {
                canvas.StrokeColor = MutedBorder;
                canvas.StrokeSize = 0.5f;
                canvas.DrawEllipse(x, y, w, h);
            }
        }

        private void RRect(ICanvas canvas, float x, float y, float w, float h, float r, bool active)
        {
            canvas.FillColor = active ? GoldActive : Muted;
            canvas.FillRoundedRectangle(x, y, w, h, r);
            if (!active)
            {
                canvas.StrokeColor = MutedBorder;
                canvas.StrokeSize = 0.5f;
                canvas.DrawRoundedRectangle(x, y, w, h, r);
            }
        }
    }
}
