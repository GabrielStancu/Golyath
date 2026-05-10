# Project Context

- **Owner:** Stancu Gabriel
- **Project:** Golyath — an offline-first gym tracking app built with .NET MAUI
- **Stack:** .NET MAUI, C#, SQLite (via sqlite-net or similar), MVVM, Clean Architecture
- **Created:** 2026-05-04

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

- **Epic 12 (Data Import & Export UI):** Settings page is the last tab in the Shell TabBar. The SettingsViewModel handles all file I/O (FilePicker, Share.RequestAsync, File.WriteAllTextAsync) directly — MAUI APIs stay in the UI layer, not the service layer. IDataPortabilityService is stubbed against the interface; Kaylee wires the real implementation. `IsNotBusy` must raise `OnPropertyChanged` in `OnIsBusyChanged` so the `IsEnabled` binding on buttons refreshes correctly alongside `NotifyCanExecuteChanged`.
