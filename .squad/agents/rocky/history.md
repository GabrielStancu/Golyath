# Project Context

- **Owner:** Stancu Gabriel
- **Project:** Golyath — .NET MAUI offline-first gym tracking app
- **Stack:** .NET MAUI, C#, SQLite (sqlite-net-pcl), MVVM, Clean Architecture
- **Layers:** Core (Entities, Enums, ValueObjects, Domain Logic) · Application (Services, Use Cases, DTOs) · Infrastructure (SQLite Repositories, Import/Export, Settings) · UI (Views XAML, ViewModels MVVM)
- **Theme:** `#FFD700` gold accent, light/dark mode required from day one — full color system defined in decisions.md
- **Charting:** SkiaSharp for custom gauges (ring, speedometer), LiveCharts2 for bar/line charts — confirm before Dashboard sprint starts
- **15 Epics:** Foundation, Onboarding, Workout Logging, Exercise Library, Dashboard, History, Analytics, Smart Suggestions, Goals, Personal Records, Tagging/Notes, Import/Export, Settings, UI Polish, Testing
- **Created:** 2026-04-26

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->


### 2026-04-26 — Epic 1 Architecture Analysis

- **Single-project folder structure decided.** MAUI's platform-specific build tooling makes multi-project solutions painful for cross-platform apps. Core/, Application/, Infrastructure/, UI/ as subfolders within `src/Golyath/` is the correct call.
- **sqlite-net-pcl is the right SQLite library.** It's the MAUI/Xamarin standard, handles all four target platforms without extra wiring, ships with SQLiteAsyncConnection out of the box.
- **CommunityToolkit.Mvvm is non-negotiable.** Source-generator-based MVVM keeps the ViewModel layer clean and fast. No ViewModelBase boilerplate needed.
- **Migration strategy: integer version-based runner.** Store schema version in a `AppMetadata` table (single row). On startup, run pending migrations in order. Simple, auditable, no external deps.
- **Database path via FileSystem.AppDataDirectory.** This is the correct cross-platform path for SQLite on MAUI — not hardcoded platform paths.
- **Repository interfaces live in Application layer, NOT Infrastructure.** Implementations live in Infrastructure. This is a common Clean Arch mistake to watch for in PRs.
- **No IoC container chosen yet.** MauiProgram.cs will use .NET's built-in `IServiceCollection`. Sufficient for this app's complexity.
- **ApplicationId needs updating** — current value `com.companyname.golyath` is the template default. Flag for Gabriel to set a real identifier before any store submission.

