# Squad Decisions

## Active Decisions

### 2026-04-26: UI Visual Language — gauge-and-chart-first design
**By:** Stancu Gabriel
**What:** The app UI follows a white-card, gauge-and-chart-first visual language. Reference design uses:
- **Circular ring gauges** (hollow ring with fill arc) — map to: weekly workout goal %, readiness indicator, muscle group balance wheel
- **Arc/speedometer gauges** (semicircular arc with needle or fill) — map to: session intensity score, estimated 1RM % of goal, weekly volume vs target
- **Dual-color grouped bar charts** — map to: volume per muscle group (push/pull/legs/core), weekly activity (sets vs target)
- **Stat cards** (icon + large number + label, side-by-side pairs) — map to: today's volume, sets completed, PRs hit, rest time
- Background: near-white `#F5F5F7` (light) / `#1C1C1E` (dark)
- Cards: pure white `#FFFFFF` (light) / `#2C2C2E` (dark), 16dp corner radius, soft shadow (0 2px 8px rgba(0,0,0,0.08))
- Primary accent: `#FFD700` (gold) — replaces orange in reference
- Secondary metric colors: `#7C6AF5` (purple, e.g. push volume), `#FF7F6E` (coral, e.g. pull volume), `#4CAF50` (green, completion/success states)
- Both light and dark themes required from day one
**Why:** Gabriel provided UI reference image. Design language is established — Apollo implements against it.

### 2026-04-26: Charting library — SkiaSharp + custom drawn gauges preferred
**By:** Stancu Gabriel (via Copilot — architecture inference from design reference)
**What:** Use **SkiaSharp** for custom circular ring gauges and arc/speedometer gauges (these require pixel-level control not available in standard MAUI controls). Use **LiveCharts2** (MAUI flavor) for bar charts and line charts where standard charting suffices. Do NOT use third-party gauge controls that don't support dark mode or custom theming.
**Why:** The reference design uses non-standard gauge shapes that require custom drawing. SkiaSharp gives Apollo full control over stroke width, gradient fills, and animation. This is a pending decision — Rocky must confirm before Apollo starts Dashboard.

### 2026-04-26: No auto-commit — Gabriel approves after testing
**By:** Stancu Gabriel
**What:** Agents must NOT auto-commit at the end of their workflow. Gabriel reviews and approves all work once tested before committing to git. This applies to all agents including Scribe.
**Why:** User directive — Gabriel wants explicit control over what enters version history.

---

### ADR-001: Single-Project Folder Structure
**By:** Rocky (2026-04-26) | **Status:** Active

Use subfolders within `src/Golyath/` rather than separate class library projects. Layers enforced via namespace conventions: `Core/`, `Application/`, `Infrastructure/`, `UI/`. MAUI's build system is tightly coupled to a single project; multi-project solutions add friction with no architectural gain at this scale. Revisit if any layer exceeds ~50 classes.

---

### ADR-002: SQLite via sqlite-net-pcl
**By:** Rocky (2026-04-26) | **Status:** Active

`sqlite-net-pcl` + `SQLitePCLRaw.bundle_green`. Provides async-first `SQLiteAsyncConnection`, attribute-based table mapping, and cross-platform support. EF Core rejected (too heavyweight for MAUI), LiteDB rejected (ecosystem fit).

---

### ADR-003: MVVM via CommunityToolkit.Mvvm
**By:** Rocky (2026-04-26) | **Status:** Active

Source-generator-based `[ObservableProperty]` and `[RelayCommand]` via `CommunityToolkit.Mvvm`. Eliminates ViewModelBase boilerplate. Maintained by Microsoft; integrates cleanly with MAUI data binding.

---

### ADR-004: Integer-Versioned Migration Runner
**By:** Rocky (2026-04-26) | **Status:** Active

`AppMetadata` table stores `schema_version`. `DatabaseMigrationRunner` holds an ordered `List<IMigration>`; applies all migrations where `Version > currentVersion` on startup, then persists new version. Location: `Infrastructure/Database/Migrations/`. `CreateTableAsync<AppMetadata>()` is idempotent (IF NOT EXISTS).

---

### ADR-005: Repository Pattern Conventions
**By:** Rocky (2026-04-26) | **Status:** Active

Interfaces in `Application/Interfaces/`, implementations in `Infrastructure/Database/Repositories/`. All methods `async Task<T>`. Generic base `IRepository<T>` extended by domain-specific interfaces. Injected via constructor DI through `IServiceCollection`.

---

### ADR-006: Database Path via FileSystem.AppDataDirectory
**By:** Rocky (2026-04-26) | **Status:** Active

```csharp
var dbPath = Path.Combine(FileSystem.AppDataDirectory, "golyath.db3");
```
Cross-platform MAUI API resolving to app-private storage on each platform.

---

### ADR-007: DI Container — .NET built-in IServiceCollection
**By:** Rocky (2026-04-26) | **Status:** Active

No third-party IoC container. All repositories, services, and ViewModels registered in `MauiProgram.cs`. Repositories as `Scoped`; `DatabaseContext` as `Singleton`.

---

### 2026-04-26: CreateMauiApp async pattern
**By:** Mickey (2026-04-26) | **Status:** Active

`CreateMauiApp()` stays synchronous (MAUI framework contract). DB migration runs via private `CreateMauiAppAsync()` called with `.GetAwaiter().GetResult()`. Fire-and-forget lifecycle hook rejected — a failed migration must block startup.

---

### 2026-04-26: JSON string storage for collection properties
**By:** Mickey (2026-04-26) | **Status:** Active

`SecondaryMuscles` (Exercise) and `Tags` (Workout) stored as JSON strings. sqlite-net-pcl does not support collection navigation properties. Parsing delegated to Application layer. Can be normalised via a new migration if needed.

---

### 2026-04-26: net9.0 TFM + dual-constructor pattern for testability
**By:** Mickey (in response to Duke) (2026-04-26) | **Status:** Active

`net9.0` prepended to `Golyath.csproj` `TargetFrameworks`. MAUI-specific items (`UseMaui`, `SingleProject`, `OutputType`, all `Maui*` resources, `Microsoft.Maui.Controls` package, MAUI source files) conditionalized on `'$(TargetFramework)' != 'net9.0'`. All four repositories (`SqliteUserRepository`, `SqliteExerciseRepository`, `SqliteWorkoutRepository`, `SqliteSetRepository`) expose a secondary constructor accepting `SQLiteAsyncConnection` directly, bypassing `DatabaseContext` and `FileSystem.AppDataDirectory` for test use.

---

### 2026-04-26: Views live under UI/Views/ with namespace Golyath.UI.Views
**By:** Apollo (2026-04-26) | **Status:** Active

All `ContentPage` / `ContentView` files must live under `src/Golyath/UI/Views/` (or a sub-folder) with namespace `Golyath.UI.Views`. `AppShell.xaml` references them via `xmlns:views="clr-namespace:Golyath.UI.Views"`. MAUI glob includes handle the new path automatically — no `.csproj` edits required when adding Views.

---

### Flagged for Gabriel (open items)
- `ApplicationId` is currently `com.companyname.golyath` — must be set to a real identifier before any device or store deployment.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
