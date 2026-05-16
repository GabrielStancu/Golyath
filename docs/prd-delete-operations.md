# PRD: Delete Operations

## Problem Statement

Users cannot delete their own data in most parts of the app. There is no way to remove a completed workout from history, delete individual sets during an active workout, or delete custom exercises from the exercise library. The only deletion path that exists — removing an exercise from an active workout — has no confirmation, making accidental data loss silently irreversible. The underlying data model also has a cascade bug: deleting a workout or a workout exercise orphans its child records (WorkoutSets, WorkoutExercises, WorkoutTags) in the database.

## Solution

Add intentional, guarded delete entry points at every level of the data hierarchy where deletion makes sense: whole workouts (from history), exercises (from the active workout), sets (from the active workout), and custom exercises (from the exercise library). All destructive operations that affect multiple records use a cascade delete strategy — children are removed before parents, within the service layer. Dangerous deletions are protected by a confirmation dialog (`ConfirmPopup` with `isDestructive: true`). History remains read-only except for whole-workout deletion.

## User Stories

1. As a user, I want to delete a completed workout by swiping its row in the History list, so that I can quickly remove sessions I logged by mistake.
2. As a user, I want to delete a completed workout from the Workout Detail page, so that I can review the session before confirming removal.
3. As a user, I want to see a confirmation dialog before a completed workout is permanently deleted, so that I don't lose data by accident.
4. As a user, I want the app to automatically remove all exercises, sets, and tags associated with a deleted workout, so that my database stays clean and consistent.
5. As a user, I want to delete an individual set from an active workout by swiping it, so that I can correct logging errors without interrupting my flow.
6. As a user, I want to remove an exercise from an active workout, so that I can drop movements I no longer want to do that session.
7. As a user, I want to see a confirmation dialog before an exercise is removed from my active workout, so that I don't accidentally lose all logged sets for that exercise.
8. As a user, I want all logged sets for an exercise to be automatically deleted when I remove that exercise from an active workout, so that no orphaned data is left behind.
9. As a user, I want to delete a custom exercise I created from the Exercise Detail page, so that I can clean up exercises I no longer use.
10. As a user, I want to see a confirmation dialog before a custom exercise is deleted, so that I understand the action is permanent.
11. As a user, I want the app to warn me if a custom exercise I am about to delete has been used in past workouts, so that I can make an informed decision.
12. As a user, I want all past WorkoutExercise and WorkoutSet records for a deleted custom exercise to be automatically removed, so that history does not reference a non-existent exercise.
13. As a user, I want the delete button on the Exercise Detail page to only appear for custom exercises, so that I cannot accidentally remove built-in exercises from the library.
14. As a user, I want the workout history detail view to remain read-only for individual exercises and sets, so that the history accurately reflects what I actually did.
15. As a user, I want duplicate set and delete set swipe gestures to be clearly separated (duplicate on one side, delete on the other), so that I do not confuse the two actions.
16. As a user, I want abandoned active workouts to also cascade-delete their child records, so that incomplete sessions leave no orphaned data.

## Implementation Decisions

### Module 1: `WorkoutService` — Cascade Delete (new `DeleteWorkoutAsync`, fixed `AbandonWorkoutAsync`, fixed `RemoveExerciseAsync`, new `RemoveSetAsync`)

`WorkoutService` is the single place that owns all cascade delete logic for workout-related records. No cascade logic lives in repositories.

**New / changed methods on `IWorkoutService`:**

- `DeleteWorkoutAsync(int workoutId)` — deletes a completed workout and all child records in dependency order: WorkoutSets → WorkoutExercises → WorkoutTags → Workout. Also used internally by the abandon flow.
- `AbandonWorkoutAsync(int workoutId)` — fixed to call the shared cascade path instead of a bare `DeleteAsync` on the Workout row.
- `RemoveExerciseAsync(int workoutExerciseId)` — fixed to first delete all WorkoutSets for that WorkoutExercise before deleting the WorkoutExercise itself.
- `RemoveSetAsync(int setId)` — new method, deletes a single WorkoutSet by id.

`IWorkoutHistoryService` gains `DeleteWorkoutAsync(int workoutId)` which delegates to `WorkoutService.DeleteWorkoutAsync`. This keeps the history service as the public API for history-screen ViewModels, without duplicating cascade logic.

`WorkoutService` currently only receives `IWorkoutRepository`, `IWorkoutExerciseRepository`, and `IWorkoutSetRepository`. To cascade WorkoutTags it will also need `IWorkoutTagRepository`.

### Module 2: `ExerciseService` — Delete Custom Exercise (new `DeleteCustomExerciseAsync`)

A new method `DeleteCustomExerciseAsync(int exerciseId)` on `IExerciseService` (and implementation). It must:
1. Guard: only proceed if the exercise has `IsCustom == true`.
2. Load all `WorkoutExercise` records referencing this exercise.
3. For each, delete all child `WorkoutSet` records, then the `WorkoutExercise`.
4. Delete the `Exercise` entity itself.

`ExerciseService` will need `IWorkoutExerciseRepository` and `IWorkoutSetRepository` injected alongside the existing `IExerciseRepository`.

A separate query is needed to determine whether the exercise appears in any past workouts, so the `ExerciseDetailViewModel` can surface a richer warning in the confirmation message before the user commits.

### Module 3: `HistoryViewModel` — Swipe-to-Delete on History List

- New `DeleteWorkoutCommand(WorkoutHistorySummaryDto workout)` marked `[RelayCommand]`.
- Shows `ConfirmPopup` before proceeding.
- On confirm, calls `IWorkoutHistoryService.DeleteWorkoutAsync`, then removes the item from the `Workouts` observable collection.
- The `HistoryPage` XAML gains a `SwipeView` wrapping each history row, with a left-side red delete `SwipeItem`.

### Module 4: `WorkoutDetailViewModel` — Delete Workout Button on Detail Page

- New `DeleteWorkoutCommand` marked `[RelayCommand]`.
- Shows `ConfirmPopup` before proceeding.
- On confirm, calls `IWorkoutHistoryService.DeleteWorkoutAsync`, then navigates back to the History list.
- The `WorkoutDetailPage` XAML gains a trash icon `ImageButton` in the page toolbar, conditionally shown (always shown — detail page is only reachable for completed workouts).

### Module 5: `ActiveWorkoutViewModel` — Confirmation on Remove Exercise

- The existing `OnRemoveExercise` handler (which handles the `RemoveRequested` event from `WorkoutExerciseViewModel`) is updated to show a `ConfirmPopup` with `isDestructive: true` before calling `IWorkoutService.RemoveExerciseAsync`.
- The fixed `RemoveExerciseAsync` in the service handles cascade to sets automatically; no ViewModel changes needed there.

### Module 6: `WorkoutExerciseViewModel` — Delete Set Command

- New `RemoveSetCommand(WorkoutSetViewModel set)` marked `[RelayCommand]`.
- Calls `IWorkoutService.RemoveSetAsync(set.SetId)` and removes the item from the `Sets` observable collection.
- No confirmation (sets are small, low-risk, and users are in a flow state at the gym).
- The `ActiveWorkoutPage` XAML `SwipeView` on set rows gains a new left-side red delete `SwipeItem` bound to `RemoveSetCommand`. The existing duplicate swipe item stays on the right.

### Module 7: `ExerciseDetailViewModel` — Delete Custom Exercise

- New `DeleteExerciseCommand` marked `[RelayCommand]` and only enabled when the loaded `Exercise.IsCustom == true`.
- Queries whether the exercise appears in past workouts and builds an appropriate warning message for `ConfirmPopup`.
- On confirm, calls `IExerciseService.DeleteCustomExerciseAsync` and navigates back to the Exercise Library page.
- The `ExerciseDetailPage` XAML gains a trash icon `ImageButton` in the page toolbar, visible only when `IsCustom`.

### Schema / Data Integrity Notes

- No schema changes are required. All cascade deletes are performed at the application layer (SQLite-net-pcl does not enforce foreign key constraints by default).
- Deletion order must always be: WorkoutSets → WorkoutExercises → WorkoutTags → Workout (leaves before root).
- The `AbandonWorkoutAsync` fix must not break the existing confirmation UX in `ActiveWorkoutViewModel`; only the service-layer implementation changes.

## Testing Decisions

**What makes a good test here:** Tests should exercise the service-layer methods directly, asserting on the observable state of the database (what records remain) rather than on internal method calls. ViewModels should be tested by constructing them with in-memory or fake services and asserting that their observable collections update correctly after a delete.

**Modules to test:**

- `WorkoutService.DeleteWorkoutAsync` — given a workout with exercises, sets, and tags, assert that all child records and the workout itself are gone after the call. Assert that a non-existent id is a no-op (no exception).
- `WorkoutService.AbandonWorkoutAsync` — assert the same cascade behavior as `DeleteWorkoutAsync` now that they share the same path.
- `WorkoutService.RemoveExerciseAsync` — assert that all WorkoutSets for the given WorkoutExercise are deleted alongside the WorkoutExercise. Assert that other exercises' sets are unaffected.
- `WorkoutService.RemoveSetAsync` — assert the specific set is deleted and sibling sets are unaffected.
- `ExerciseService.DeleteCustomExerciseAsync` — assert that a custom exercise with past WorkoutExercise/Set records is fully cascade-deleted. Assert that calling it on a non-custom exercise is a no-op or throws a domain exception.

**Prior art:** The codebase currently has no automated tests. The above service tests should follow an in-memory SQLite pattern (using a `:memory:` connection via `DatabaseService`) so they are fast, isolated, and require no mocking of repositories.

## Out of Scope

- Editing individual sets or exercises within a past workout — history is intentionally read-only except for whole-workout deletion.
- Deleting built-in (seeded) exercises — only custom exercises can be deleted.
- Deleting tag entities themselves — users can only unlink tags from workouts; the tag catalogue is not managed here.
- A "wipe all data" / delete account option in Settings.
- Undo / soft delete — all deletions in this PRD are permanent.
- Any cloud sync implications.

## Further Notes

- The `ConfirmPopup` control already exists and supports `isDestructive: true` (used by goal delete and workout abandon). All new confirmations should follow the same pattern.
- `WorkoutService` will need `IWorkoutTagRepository` added as a constructor dependency to support cascade tag deletion. The DI registration in `ServiceCollectionExtensions` must be updated accordingly.
- The existing `WorkoutExerciseViewModel.RemoveCommand` raises a `RemoveRequested` event and does not call any service directly — the actual service call happens in `ActiveWorkoutViewModel`. This event pattern should be kept as-is; the confirmation dialog is added in `ActiveWorkoutViewModel`'s handler.
- Swipe directions should follow platform conventions: delete on the left swipe edge (consistent with iOS standard), duplicate on the right.
