# Project Context

- **Owner:** Stancu Gabriel
- **Project:** Golyath — .NET MAUI offline-first gym tracking app
- **Stack:** C#, sqlite-net-pcl, Clean Architecture (Core / Application / Infrastructure / UI)
- **DB tables:** Users · Exercises · Workouts · WorkoutExercises · Sets · Goals · Tags
- **Exercise source:** Open-source JSON dataset in /exercises/ folder (100+ exercises with muscle metadata)
- **Key constraints:** 100% offline, no data loss, async everywhere, batch writes during workout, schema migrations required
- **Created:** 2026-04-26

## Learnings

### 2026-04-26 — Epic 1 Foundation (T1.1–T1.9)

**Architecture applied:**
- Folder scaffold: Core/Entities, Core/Enums, Core/ValueObjects, Application/{Interfaces,Services,UseCases,DTOs}, Infrastructure/Database/{Migrations,Repositories}, Infrastructure/Settings, UI/{Views,ViewModels,Converters,Controls} — each with `.gitkeep`
- NuGet: `sqlite-net-pcl` 1.9.172, `SQLitePCLRaw.bundle_green` 2.1.10, `CommunityToolkit.Mvvm` 8.4.0 added to `Golyath.csproj`
- Migration strategy: integer-versioned `IMigration` interface + `DatabaseMigrationRunner`; schema version stored in `AppMetadata` table under key `schema_version`
- `DatabaseContext` uses double-checked locking with `SemaphoreSlim` for thread-safe lazy connection init; db stored at `FileSystem.AppDataDirectory/golyath.db3`

**Key files created:**
- `Core/Entities/`: User, Exercise, Workout, WorkoutExercise, Set, Goal, Tag, AppMetadata
- `Core/Enums/`: FitnessGoal, MovementType, EquipmentType, Gender, WeightUnit, GoalType
- `Application/Interfaces/`: IRepository<T>, IUserRepository, IExerciseRepository, IWorkoutRepository, ISetRepository, IDatabaseMigrationRunner, IMigration
- `Infrastructure/Database/DatabaseContext.cs`
- `Infrastructure/Database/Migrations/Migration_0001_InitialSchema.cs`
- `Infrastructure/Database/Migrations/DatabaseMigrationRunner.cs`
- `Infrastructure/Database/Repositories/`: SqliteRepository<T> (base), SqliteUserRepository, SqliteExerciseRepository, SqliteWorkoutRepository, SqliteSetRepository

**Notable implementation decision:**
- MAUI platform delegates (`AppDelegate`, `MainApplication`) override `CreateMauiApp()` returning `MauiApp` synchronously — async migration can't be awaited directly. Used public sync `CreateMauiApp()` → calls private `CreateMauiAppAsync().GetAwaiter().GetResult()` pattern to keep the async migration while satisfying the platform contract.

**DI registration:** `DatabaseContext` and `DatabaseMigrationRunner` as singletons; all four repositories as scoped.

### 2026-04-26 — Testability Fixes (Duke's gaps)

**Testability gaps fixed:**
- `Golyath.csproj` now includes `net9.0` as an explicit TFM so test projects (`net9.0`) can resolve the `ProjectReference`; `UseMaui`, `SingleProject`, `OutputType=Exe`, MAUI resource items, and `Microsoft.Maui.Controls` are all conditionalized to `'$(TargetFramework)' != 'net9.0'`; MAUI source files (`App.xaml.cs`, `AppShell.xaml.cs`, `MauiProgram.cs`, `GlobalXmlns.cs`, `UI/Views/**/*.xaml.cs`, `Platforms/**/*.cs`) are excluded from the `net9.0` build via `<Compile Remove=...>`
- `SqliteRepository<T>` refactored to dual-constructor pattern: primary constructor takes `DatabaseContext` (MAUI/DI path); secondary constructor takes `SQLiteAsyncConnection` directly (test path); all CRUD methods now route through `GetConnectionAsync()` helper that selects the correct source; `Context` field changed to `protected readonly DatabaseContext?`
- All 4 concrete repos (`SqliteUserRepository`, `SqliteExerciseRepository`, `SqliteWorkoutRepository`, `SqliteSetRepository`) expose the `SQLiteAsyncConnection` overload constructor; their custom query methods updated to call `GetConnectionAsync()` instead of `Context.GetConnectionAsync()`; `using SQLite;` added to each
- DI wiring in `MauiProgram.cs` is unchanged — production path unaffected

