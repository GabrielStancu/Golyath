# Project Context

- **Owner:** Stancu Gabriel
- **Project:** Golyath — an offline-first gym tracking app built with .NET MAUI
- **Stack:** .NET MAUI, C#, SQLite (via sqlite-net or similar), MVVM, Clean Architecture
- **Created:** 2026-05-04

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-05-10 — DataPortabilityService test project setup

- Created `src/Golyath.Tests/` as the first test project in the solution.
- Test project targets `net9.0-windows10.0.19041.0` to match the MAUI Windows target so tests can run without mobile SDKs.
- Uses xUnit + NSubstitute 5.1.0. All 8 repository interfaces are mocked via `Substitute.For<T>()`.
- xUnit instantiates a fresh test class per test method, so mock state is isolated without `[SetUp]` teardown — defaults are set in the constructor.
- `IWorkoutTagRepository.GetAllAsync()` is referenced in tests but does not exist yet in the interface; Kaylee must add it for tests to compile.
- `BackupDocument`, `BackupData`, all backup records, `IDataPortabilityService`, and `DataPortabilityService` are Kaylee's deliverables — tests are intentionally red until her branch merges (TDD red-green).
- Duplicate detection design: users matched by Nickname, tags matched by Name, seeded exercises matched by ExternalId. These are behavioural contracts verified by the tests.
- Run command: `dotnet test src/Golyath.Tests/Golyath.Tests.csproj --framework net9.0-windows10.0.19041.0`
