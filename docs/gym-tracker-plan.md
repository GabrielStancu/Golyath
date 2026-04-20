# Plan: .NET MAUI Gym Tracker App (Local-Only)

## TL;DR

A local-only .NET MAUI gym tracking app for Android + iOS. Pre-seeded SQLite database with 200+ exercises and images. Users create workout templates/routines, log sets/reps/weight/tempo, track progression via charts, analyze muscle group distribution, and export data for backup. Built with MVVM, sqlite-net, and LiveCharts2.

---

## Architecture & Tech Stack

- **.NET MAUI** (net9.0-android / net9.0-ios)
- **SQLite** via `sqlite-net-pcl` — simple, performant, well-supported on mobile
- **CommunityToolkit.Mvvm** — source-generated MVVM (ObservableProperty, RelayCommand)
- **MAUI CommunityToolkit** — converters, behaviors, popups
- **LiveCharts2 (SkiaSharp)** — rich charts for progression & distribution
- **MVVM + Shell navigation**

---

## Data Model (SQLite Tables)

### Reference Data (pre-seeded)

- **MuscleGroup** — Id, Name, DisplayName, BodyRegion (Upper/Lower/Core), IconPath
- **Equipment** — Id, Name (Barbell, Dumbbell, Cable, Machine, Bodyweight, Band, Kettlebell)
- **Exercise** — Id, Name, Description, PrimaryMuscleGroupId (FK), EquipmentId (FK), ImagePath, IsCustom (bool), IsCompound (bool)
- **ExerciseMuscleGroup** — ExerciseId, MuscleGroupId, Role (Primary/Secondary/Stabilizer) — many-to-many for secondary muscles

### User Data

- **WorkoutTemplate** — Id, Name, Description, CreatedAt, LastUsedAt, Color
- **WorkoutTemplateEntry** — Id, TemplateId (FK), ExerciseId (FK), SortOrder, TargetSets, TargetReps, TargetWeight, TargetTempo, RestSeconds
- **WorkoutSession** — Id, TemplateId (FK, nullable for freeform), StartedAt, FinishedAt, Notes, Rating (1-5)
- **WorkoutSet** — Id, SessionId (FK), ExerciseId (FK), SetNumber, Reps, Weight, Tempo (string "3-1-2-0"), IsWarmup (bool), RPE (nullable), CompletedAt

### Derived/Computed

- Volume = Sets × Reps × Weight (computed at query time)
- Muscle group distribution = aggregate ExerciseMuscleGroup joins over WorkoutSet in a date range

---

## App Pages & Navigation (Shell)

### 1. Dashboard (Home Tab)

- Quick stats: workouts this week, total volume, streak
- Next scheduled workout (if using a program)
- Recent workout summary card
- Muscle group heatmap (last 7/30 days) — body outline or radar chart showing which groups are trained most/least

### 2. Workout (Active Session) Tab

- Start from template or freeform
- List of exercises with expandable set rows
- Each set row: weight input, reps input, tempo input (optional), warmup toggle
- Swipe to delete set, tap + to add set
- **Rest timer**: auto-starts on set completion, configurable per exercise (from template defaults), countdown overlay or notification
- Running duration clock at top
- Finish button → save session, optional notes and rating

### 3. History Tab

- Calendar view (dots on workout days) + list view toggle
- Tap a day → see session details (exercises, sets, total volume)
- Tap a session → full breakdown with per-exercise volume

### 4. Exercises Tab

- Searchable, filterable list (by muscle group, equipment type)
- Exercise detail page: image, description, primary/secondary muscles, personal records, progression chart for that exercise
- Ability to add custom exercises (with camera photo or from gallery)

### 5. Analytics Tab

- **Progression charts**: line chart of estimated 1RM or total volume per exercise over time
- **Muscle group distribution**: radar/spider chart or horizontal bar chart showing relative volume per muscle group (last 7/14/30/90 days)
  - Color-coded: green (balanced), yellow (slightly under), red (neglected)
- **Workout frequency**: bar chart of sessions per week over time
- **Volume trends**: total weekly volume line chart
- **Personal records** feed: timeline of PRs

### 6. Settings Page

- Units (kg/lbs)
- Default rest timer duration
- Export data (CSV / JSON)
- Import data (JSON)
- Reset database
- About / version

---

## Exercise Library & Images

### Sourcing ~200+ exercises

- Seed from the **wger Exercise Database** (open-source, CC-BY-SA licensed, has exercise names, descriptions, muscle groups, and exercise images)
  - https://wger.de/api/v2/ — free API with CC-BY-SA 3.0 data
  - Download and bundle the data at build time into a pre-seeded SQLite DB
- Alternatively, use **free SVG exercise illustrations** from open-source fitness projects
- Ship images as bundled assets in `Resources/Raw/exercises/` (~2-5 KB per optimized WebP image, 200 images ≈ 0.5-1 MB)

### Pre-seeded DB strategy

- Create a `seed.db` SQLite file during development with all exercises + muscle groups + equipment
- Include it in `Resources/Raw/`
- On first launch, copy to `FileSystem.AppDataDirectory` if no DB exists
- On app update, use a version number to merge new exercises into existing DB (preserving user data)

---

## Key Implementation Details

### Tempo tracking

- Store as string format "E-P1-C-P2" (Eccentric-Pause-Concentric-Pause), e.g., "3-1-2-0"
- Custom input control: 4 small numeric steppers in a row
- Optional per set — most users won't use it every time

### Rest timer

- `IDispatcherTimer` for countdown
- Configurable default per template entry (override per-session)
- Auto-start when a set is marked complete
- Local notification when timer expires (if app is backgrounded)
- Vibration + sound on completion

### Muscle group distribution analysis

- Query: JOIN WorkoutSet → Exercise → ExerciseMuscleGroup → MuscleGroup
- Weight the contribution: Primary muscle = 1.0×, Secondary = 0.5×, Stabilizer = 0.25×
- Aggregate volume (sets × reps × weight × muscle_weight_factor) per muscle group
- Compare against a "balanced" baseline and flag over/under-trained groups
- Display as radar chart (LiveCharts2) or body heatmap

### Progression tracking

- For each exercise, track estimated 1RM over time (Epley formula: weight × (1 + reps/30))
- Show line chart with date on X-axis, e1RM on Y-axis
- Also show raw volume (sets × reps × weight) trend

### Data export/import

- Export: serialize WorkoutSession + WorkoutSet data to JSON (or CSV per table)
- Store export file via `FileSaver` (MAUI CommunityToolkit)
- Import: read JSON, insert/merge into SQLite
- This is the backup/restore mechanism since there's no backend

---

## Project Structure

```
GymTracker/
├── GymTracker.sln
├── GymTracker/
│   ├── App.xaml / App.xaml.cs
│   ├── AppShell.xaml / AppShell.xaml.cs
│   ├── MauiProgram.cs                     # DI registration
│   ├── Models/                            # SQLite entity classes
│   │   ├── MuscleGroup.cs
│   │   ├── Equipment.cs
│   │   ├── Exercise.cs
│   │   ├── ExerciseMuscleGroup.cs
│   │   ├── WorkoutTemplate.cs
│   │   ├── WorkoutTemplateEntry.cs
│   │   ├── WorkoutSession.cs
│   │   └── WorkoutSet.cs
│   ├── Data/                              # Database layer
│   │   ├── GymTrackerDatabase.cs          # SQLiteAsyncConnection, table init, seed logic
│   │   └── SeedData/
│   │       └── seed.db                    # Pre-built SQLite with exercises
│   ├── Services/                          # Business logic
│   │   ├── IWorkoutService.cs
│   │   ├── WorkoutService.cs
│   │   ├── IExerciseService.cs
│   │   ├── ExerciseService.cs
│   │   ├── IAnalyticsService.cs
│   │   ├── AnalyticsService.cs
│   │   ├── IExportService.cs
│   │   ├── ExportService.cs
│   │   └── ITimerService.cs / TimerService.cs
│   ├── ViewModels/                        # CommunityToolkit.Mvvm ViewModels
│   │   ├── DashboardViewModel.cs
│   │   ├── ActiveWorkoutViewModel.cs
│   │   ├── HistoryViewModel.cs
│   │   ├── ExerciseListViewModel.cs
│   │   ├── ExerciseDetailViewModel.cs
│   │   ├── AnalyticsViewModel.cs
│   │   └── SettingsViewModel.cs
│   ├── Views/                             # XAML pages
│   │   ├── DashboardPage.xaml
│   │   ├── ActiveWorkoutPage.xaml
│   │   ├── HistoryPage.xaml
│   │   ├── ExerciseListPage.xaml
│   │   ├── ExerciseDetailPage.xaml
│   │   ├── AnalyticsPage.xaml
│   │   └── SettingsPage.xaml
│   ├── Controls/                          # Custom controls
│   │   ├── TempoInput.xaml                # 4-digit tempo picker
│   │   ├── RestTimerOverlay.xaml
│   │   ├── MuscleHeatmap.xaml             # Body outline SVG with color-coding
│   │   └── SetEntryRow.xaml               # Reusable set input row
│   ├── Converters/                        # Value converters
│   ├── Resources/
│   │   ├── Raw/
│   │   │   └── exercises/                 # Exercise images (WebP)
│   │   ├── Styles/
│   │   └── Fonts/
│   └── Platforms/
│       ├── Android/
│       └── iOS/
```

---

## Phases

### Phase 1 — Foundation (MVP)

1. Set up MAUI project, Shell navigation, DI
2. Define SQLite models and `GymTrackerDatabase` (table creation, seed copy logic)
3. Build and populate seed.db with ~50 exercises (expand later) from wger data
4. Exercise list/detail pages with search and filter
5. Active workout page: start session, add exercises, log sets (weight + reps)
6. Save session to WorkoutSession + WorkoutSet tables
7. History page: list of past sessions with basic detail view

### Phase 2 — Templates & Timer

8. Workout template CRUD (create, edit, delete, reorder exercises)
9. Start workout from template (pre-populate exercises + target sets)
10. Rest timer with countdown, notification, and vibration
11. Tempo input control (optional per set)

### Phase 3 — Analytics & Progression

12. Progression chart per exercise (e1RM line chart via LiveCharts2)
13. Total volume trend chart (weekly)
14. Muscle group distribution radar/bar chart with balance indicators
15. Workout frequency chart
16. Personal records detection and feed
17. Dashboard with quick stats, streak, heatmap

### Phase 4 — Polish & Export

18. Data export (JSON) with FileSaver
19. Data import (JSON) with merge logic
20. Expand exercise library to 200+ exercises with images
21. Custom exercise creation (with camera/gallery photo)
22. Units preference (kg/lbs) with conversion
23. UI polish: animations, dark/light theme, onboarding

---

## Additional Ideas Worth Considering

1. **Workout programs/schedules** — multi-week programs (e.g., PPL, Upper/Lower) that schedule templates across days. Could be Phase 5.
2. **Superset / circuit support** — group exercises that are performed back-to-back. Model as a GroupId on WorkoutTemplateEntry.
3. **Body measurements tracking** — weight, body fat %, arm circumference etc. over time. Separate table + chart page.
4. **Plate calculator** — given a target weight and bar weight, show which plates to load on each side. Simple utility page.
5. **Workout sharing** — export a template as a QR code or shareable JSON file that another user can import.
6. **Apple Health / Google Fit integration** — write workout data to the platform health APIs. MAUI has community packages for this.
7. **Widget** — Android/iOS home screen widget showing today's scheduled workout or streak count.

---

## Decisions & Scope

- **In scope**: exercise library, workout logging, templates, rest timer, progression charts, muscle distribution, export/import
- **Out of scope**: backend/sync, social features, workout programs/scheduling (future phase), Apple Health integration (future phase)
- **SQLite over EF Core**: sqlite-net-pcl is lighter, faster startup, better suited for mobile. EF Core is overkill for this schema.
- **LiveCharts2 over Microcharts**: LiveCharts2 has MAUI-native support, richer chart types (radar, line, bar), and active maintenance.
- **Image format**: WebP for smallest size with quality. Resize to ~300×300px max.

---

## Verification

1. Run on Android emulator and iOS simulator — verify Shell navigation, page rendering, and SQLite operations
2. Verify seed.db is correctly copied on first launch and exercises load in the list
3. Log a full workout session (3 exercises, 3-4 sets each) — verify data persists across app restarts
4. Create a template, start a workout from it, verify pre-populated exercises
5. Verify rest timer fires notification when app is backgrounded
6. Verify progression chart shows correct data points for an exercise logged multiple times
7. Verify muscle group distribution correctly weights primary vs secondary muscles
8. Export data to JSON, delete app data, import JSON — verify data restored
9. Test with 200+ exercises — verify search/filter performance is acceptable (<100ms)
10. Test on physical devices (Android + iOS) for real-world performance
