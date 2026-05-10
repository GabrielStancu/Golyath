# Squad Decisions

## Active Decisions

---

### 2026-05-04: User directive — no auto-commit

**By:** Stancu Gabriel (via Copilot)
**What:** Scribe must NOT auto-commit `.squad/` changes after agent work. Gabriel will commit manually when he accepts changes.
**Why:** User request — captured at team initialization for team memory.

---

### 2026-05-10: Epic 12 — Data Import & Export Service Design

**Author:** Kaylee (Backend Developer)

1. **BackupSchemaVersion = 1** — versioned independently of the DB migration version. On import, any version ≠ 1 is rejected with a clear error message.

2. **Export includes ALL exercises (seeded + custom)** — both `IsCustom = false` (seeded) and `IsCustom = true` (custom) exercises are included in the backup. The `IsCustom` flag allows the importer to apply the correct dedup strategy. Seeded exercises are never re-inserted during import (matched by `ExternalId` only); custom exercises are inserted if no name match exists.

3. **Service has no MAUI API dependencies** — `IDataPortabilityService` works purely with JSON strings. File picking, file writing, and sharing are the ViewModel's responsibility. This keeps the service unit-testable without a MAUI host.

4. **`IWorkoutTagRepository.GetAllAsync()` added** — the interface previously had no bulk-read method. Added `Task<IReadOnlyList<WorkoutTag>> GetAllAsync()` to support export. Implementation reads `db.Table<WorkoutTag>().ToListAsync()`.

---

### 2026-05-10: Settings Page — Data & Backup (Epic 12 UI)

**Author:** Wash (UI Developer)

1. **Settings page is a Shell tab** — added as the last `<ShellContent>` in the `TabBar` in `AppShell.xaml`. No modal/push routing needed.

2. **ViewModel handles all file I/O** — `SettingsViewModel` calls `FilePicker.Default.PickAsync`, `Share.RequestAsync`, and `File.WriteAllTextAsync` directly. MAUI platform APIs do not bleed into the service layer (`IDataPortabilityService`).

3. **No MAUI APIs in the service layer** — `IDataPortabilityService` only deals with JSON strings. The ViewModel is responsible for picking files, reading them, writing them, and sharing them. This keeps the service testable without a MAUI runtime.

---

### 2026-05-10: Golyath.Tests Project Setup

**Author:** Zoe (Tester)

- **Target framework:** `net9.0-windows10.0.19041.0` — matches the MAUI Windows target. No mobile SDKs required; tests run locally on any Windows dev machine with .NET 9.
- **Mocking library:** NSubstitute 5.1.0 — all repository interfaces mocked, no real SQLite touched.
- **Test runner:** xUnit 2.9.0 + xunit.runner.visualstudio 2.8.2
- **Coverage:** coverlet.collector 6.0.0 included for optional coverage runs.
- **Solution file:** `src/Golyath.slnx` updated to include `Golyath.Tests/Golyath.Tests.csproj`.
- **`ExcludeAssets="runtime"`** on project reference prevents MAUI host assembly conflicts in the test runner.

Run command:
```bash
dotnet test src/Golyath.Tests/Golyath.Tests.csproj --framework net9.0-windows10.0.19041.0
```

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
