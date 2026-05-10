# Project Context

- **Owner:** Stancu Gabriel
- **Project:** Golyath — an offline-first gym tracking app built with .NET MAUI
- **Stack:** .NET MAUI, C#, SQLite (via sqlite-net or similar), MVVM, Clean Architecture
- **Created:** 2026-05-04

## Learnings

- Epic 12 (Data Import & Export): `IWorkoutTagRepository` does NOT extend `IRepository<WorkoutTag>` — `GetAllAsync()` had to be added separately to both interface and implementation.
- Export serialization uses `System.Text.Json` with `WriteIndented = true` and `CamelCase` naming policy. No MAUI API deps in service — ViewModel owns file I/O.
- Import uses ID remapping dictionaries (`Dictionary<int, int>`) for all FK relationships. All existing data is loaded into memory upfront to avoid N+1 queries.
- Seeded exercises (IsCustom = false) are matched by ExternalId and never inserted during import; custom exercises matched by Name.
- WorkoutTag.AddAsync is idempotent — no separate dedup needed for WorkoutTags in import.
- BackupSchemaVersion = 1 is independent of DB migration version.
- `DataPortabilityService` registered as `AddTransient` in `ServiceCollectionExtensions`.
