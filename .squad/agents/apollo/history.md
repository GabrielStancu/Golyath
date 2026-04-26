# Project Context

- **Owner:** Stancu Gabriel
- **Project:** Golyath — .NET MAUI offline-first gym tracking app
- **Stack:** .NET MAUI, C#, XAML, CommunityToolkit.Mvvm, SkiaSharp, LiveCharts2
- **UI layers:** Views (XAML ContentPages/Views) · ViewModels (ObservableObject, RelayCommand) · Shell navigation
- **Theme:** `#FFD700` gold accent (primary), `#7C6AF5` purple, `#FF7F6E` coral, `#4CAF50` green — light AND dark mode required from day one
- **Key UX goal:** Sub-100ms perceived delay, minimal taps for workout logging, no modal-heavy flows

## UI Design Reference (2026-04-26)

Gabriel provided a reference image with three screens establishing the visual language. Map to Golyath domain as follows:

### Visual components & gym mapping

| Reference component | Golyath usage |
|---|---|
| Circular ring gauge (hollow arc with fill) | Weekly workout goal %, readiness indicator, muscle group balance wheel |
| Arc/speedometer gauge (semicircular, needle or fill) | Session intensity score, estimated 1RM % of goal, weekly volume vs target |
| Dual-color grouped bar chart | Volume per muscle group (push/pull/legs/core), weekly activity (sets vs target) |
| Stat cards (icon + large number + label, pair layout) | Today's volume, sets completed, PRs hit, active rest time |

### Color system

| Token | Light | Dark | Used for |
|---|---|---|---|
| Background | `#F5F5F7` | `#1C1C1E` | Page background |
| Card surface | `#FFFFFF` | `#2C2C2E` | All cards |
| Card shadow | `rgba(0,0,0,0.08)` 0 2px 8px | none / subtle | Card elevation |
| Card radius | 16dp | 16dp | All cards |
| Accent primary | `#FFD700` | `#FFD700` | CTAs, active states, gold highlights |
| Accent purple | `#7C6AF5` | `#9D8FF7` | Push volume, download-type metrics |
| Accent coral | `#FF7F6E` | `#FF8F80` | Pull volume, upload-type metrics |
| Accent green | `#4CAF50` | `#66BB6A` | Completion, success, legs volume |
| Text primary | `#1C1C1E` | `#F5F5F7` | Headings, values |
| Text secondary | `#8E8E93` | `#8E8E93` | Labels, captions |

### Gauge implementation notes

- **Circular ring gauges:** Draw with SkiaSharp `SKCanvas`. Stroke-based arcs, no fill. Background track in `#E5E5EA` (light) / `#3A3A3C` (dark). Animated fill arc using `SKPath` with `DrawArc`. Support gradient fill on the arc stroke.
- **Speedometer/arc gauge:** SkiaSharp semicircular arc (180°), needle as `SKPath` rotated around center. Tick marks drawn programmatically.
- **Bar charts:** LiveCharts2 `CartesianChart` with `ColumnSeries`. Custom color binding per series. Disable grid lines except horizontal.
- All gauges must animate on load (0 → value in ~600ms, ease-out).
- All charts/gauges must respond to theme change without restart.

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->


### 2026-04-26 — T1.10: MainPage relocated to UI/Views layer
- Moved `MainPage.xaml` and `MainPage.xaml.cs` from project root (`Golyath/`) to `Golyath/UI/Views/`.
- Updated `x:Class` in XAML from `Golyath.MainPage` → `Golyath.UI.Views.MainPage`.
- Updated code-behind namespace from `Golyath` → `Golyath.UI.Views`.
- In `AppShell.xaml`: replaced `xmlns:local="clr-namespace:Golyath"` with `xmlns:views="clr-namespace:Golyath.UI.Views"` and updated `DataTemplate` binding from `local:MainPage` → `views:MainPage`.
- `MauiProgram.cs` had no direct `MainPage` reference — no changes needed there.
- `AppShell.xaml.cs` had no route registrations — no changes needed there.
- MAUI glob includes handle the new path automatically; no `.csproj` edits required.
