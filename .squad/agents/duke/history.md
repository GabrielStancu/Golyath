# Project Context

- **Owner:** Stancu Gabriel
- **Project:** Golyath — .NET MAUI offline-first gym tracking app
- **Stack:** C#, xUnit, sqlite-net-pcl (in-memory for tests), Clean Architecture
- **Test scope:** Domain logic · SQLite repositories · Migration correctness · Import/export fidelity · Core UI flows
- **Key coverage areas:** Suggestion engine rules, 1RM calculations, unit conversion, schema migrations, workout save/restore
- **Created:** 2026-04-26

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-26 — T1.11: Test project scaffolded (E1-US2, E1-US3, E1-US4)

**Test project structure:**
- `src/Golyath.Tests/` — targets `net9.0` (no MAUI platform suffix), xUnit 2.9.3
- `Helpers/TempDatabase.cs` — creates isolated `SQLiteAsyncConnection` per test via temp file; deleted on dispose
- `Infrastructure/DatabaseMigrationRunnerTests.cs` — 3 tests: table creation, schema version write, idempotency
- `Infrastructure/SqliteUserRepositoryTests.cs` — 4 tests: insert, get, update, delete
- `Infrastructure/SqliteExerciseRepositoryTests.cs` — 4 tests: search (match, case-insensitive, empty query), muscle group filter

**Tests written against expected Mickey implementation:**
- `Golyath.Core.Entities`: `User`, `Exercise`, `AppMetadata`
- `Golyath.Core.Enums`: `Gender`, `FitnessGoal`, `MovementType`, `EquipmentType`
- `Golyath.Infrastructure.Database.Migrations`: `Migration_0001_InitialSchema`
- `Golyath.Infrastructure.Database.Repositories`: `SqliteUserRepository`, `SqliteExerciseRepository`

**Testability gaps identified (see decisions/inbox/duke-testability-gap.md):**
1. `Golyath.csproj` has no `net9.0` TFM — test project `ProjectReference` will fail at build time.
   Fix: add `net9.0` to TargetFrameworks with MAUI items conditionalized.
2. Repository constructors take `DatabaseContext` which uses `FileSystem.AppDataDirectory` (MAUI-only).
   Fix: repositories must also accept `SQLiteAsyncConnection` directly (secondary constructor or refactor).
- Tests are compile-gated on both gaps — failure will surface immediately, not silently.

