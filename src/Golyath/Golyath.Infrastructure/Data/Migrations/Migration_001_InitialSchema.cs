using SQLite;

namespace Golyath.Infrastructure.Data.Migrations;

public class Migration_001_InitialSchema : IMigration
{
    public int Version => 1;

    public async Task UpAsync(SQLiteAsyncConnection db)
    {
        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Nickname    TEXT    NOT NULL DEFAULT '',
                Birthday    TEXT,
                HeightCm    REAL,
                WeightKg    REAL,
                Gender      INTEGER,
                FitnessGoal INTEGER,
                UnitSystem  INTEGER NOT NULL DEFAULT 0,
                OnboardingCompleted INTEGER NOT NULL DEFAULT 0,
                CreatedAt   TEXT NOT NULL,
                UpdatedAt   TEXT NOT NULL
            )");

        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Exercises (
                Id                  TEXT    PRIMARY KEY NOT NULL,
                Name                TEXT    NOT NULL DEFAULT '',
                Force               TEXT,
                Level               TEXT,
                Mechanic            TEXT,
                Equipment           TEXT,
                PrimaryMusclesJson  TEXT    NOT NULL DEFAULT '[]',
                SecondaryMusclesJson TEXT   NOT NULL DEFAULT '[]',
                InstructionsJson    TEXT    NOT NULL DEFAULT '[]',
                Category            TEXT,
                IsCustom            INTEGER NOT NULL DEFAULT 0,
                CreatedAt           TEXT    NOT NULL
            )");

        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Workouts (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId          INTEGER,
                Name            TEXT,
                StartedAt       TEXT    NOT NULL,
                CompletedAt     TEXT,
                Notes           TEXT,
                DurationSeconds INTEGER,
                CreatedAt       TEXT    NOT NULL
            )");

        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS WorkoutExercises (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                WorkoutId   INTEGER NOT NULL,
                ExerciseId  TEXT    NOT NULL,
                [Order]     INTEGER NOT NULL DEFAULT 0,
                Notes       TEXT
            )");

        await db.ExecuteAsync(@"
            CREATE INDEX IF NOT EXISTS idx_workoutexercises_workoutid
                ON WorkoutExercises(WorkoutId)");

        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS WorkoutSets (
                Id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                WorkoutExerciseId   INTEGER NOT NULL,
                SetNumber           INTEGER NOT NULL DEFAULT 1,
                WeightKg            REAL,
                Reps                INTEGER,
                Tempo               TEXT,
                Notes               TEXT,
                IsPersonalRecord    INTEGER NOT NULL DEFAULT 0,
                CompletedAt         TEXT,
                CreatedAt           TEXT    NOT NULL
            )");

        await db.ExecuteAsync(@"
            CREATE INDEX IF NOT EXISTS idx_workoutsets_workoutexerciseid
                ON WorkoutSets(WorkoutExerciseId)");

        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Goals (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId          INTEGER,
                Type            INTEGER NOT NULL,
                ExerciseId      TEXT,
                TargetValue     REAL    NOT NULL,
                StartDate       TEXT    NOT NULL,
                TargetDate      TEXT,
                AchievedDate    TEXT,
                IsActive        INTEGER NOT NULL DEFAULT 1,
                CreatedAt       TEXT    NOT NULL
            )");

        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Tags (
                Id      INTEGER PRIMARY KEY AUTOINCREMENT,
                Name    TEXT    NOT NULL,
                Color   TEXT
            )");

        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS WorkoutTags (
                WorkoutId   INTEGER NOT NULL,
                TagId       INTEGER NOT NULL,
                PRIMARY KEY (WorkoutId, TagId)
            )");
    }
}
