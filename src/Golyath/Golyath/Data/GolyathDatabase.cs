using Golyath.Models;
using SQLite;

namespace Golyath.Data;

public class GolyathDatabase
{
    private const string DatabaseFilename = "golyath.db3";
    private const string SeedAssetName = "seed.db";
    private const string SeedVersionKey = "SeedVersion";
    private const int CurrentSeedVersion = 1;

    private SQLiteAsyncConnection? _connection;

    private string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        _connection = new SQLiteAsyncConnection(DatabasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await CreateTablesAsync(_connection);
        await SeedAsync(_connection);

        return _connection;
    }

    private static async Task CreateTablesAsync(SQLiteAsyncConnection db)
    {
        await db.CreateTableAsync<MuscleGroup>();
        await db.CreateTableAsync<Equipment>();
        await db.CreateTableAsync<Exercise>();
        await db.CreateTableAsync<ExerciseMuscleGroup>();
        await db.CreateTableAsync<WorkoutTemplate>();
        await db.CreateTableAsync<WorkoutTemplateEntry>();
        await db.CreateTableAsync<WorkoutSession>();
        await db.CreateTableAsync<WorkoutSet>();
    }

    private static async Task SeedAsync(SQLiteAsyncConnection db)
    {
        int installedVersion = Preferences.Get(SeedVersionKey, 0);

        if (installedVersion >= CurrentSeedVersion)
            return;

        // Only seed reference data if tables are empty
        int muscleGroupCount = await db.Table<MuscleGroup>().CountAsync();
        if (muscleGroupCount == 0)
            await InsertReferenceDataAsync(db);

        Preferences.Set(SeedVersionKey, CurrentSeedVersion);
    }

    private static async Task InsertReferenceDataAsync(SQLiteAsyncConnection db)
    {
        // --- Equipment ---
        var equipment = new List<Equipment>
        {
            new() { Id = 1, Name = "Barbell" },
            new() { Id = 2, Name = "Dumbbell" },
            new() { Id = 3, Name = "Cable" },
            new() { Id = 4, Name = "Machine" },
            new() { Id = 5, Name = "Bodyweight" },
            new() { Id = 6, Name = "Band" },
            new() { Id = 7, Name = "Kettlebell" },
        };
        await db.InsertAllAsync(equipment);

        // --- Muscle Groups ---
        var muscleGroups = new List<MuscleGroup>
        {
            new() { Id = 1, Name = "Chest",     DisplayName = "Chest",     BodyRegion = "Upper" },
            new() { Id = 2, Name = "Back",      DisplayName = "Back",      BodyRegion = "Upper" },
            new() { Id = 3, Name = "Shoulders", DisplayName = "Shoulders", BodyRegion = "Upper" },
            new() { Id = 4, Name = "Biceps",    DisplayName = "Biceps",    BodyRegion = "Upper" },
            new() { Id = 5, Name = "Triceps",   DisplayName = "Triceps",   BodyRegion = "Upper" },
            new() { Id = 6, Name = "Legs",      DisplayName = "Legs",      BodyRegion = "Lower" },
            new() { Id = 7, Name = "Glutes",    DisplayName = "Glutes",    BodyRegion = "Lower" },
            new() { Id = 8, Name = "Core",      DisplayName = "Core",      BodyRegion = "Core"  },
            new() { Id = 9, Name = "Calves",    DisplayName = "Calves",    BodyRegion = "Lower" },
        };
        await db.InsertAllAsync(muscleGroups);

        // --- Exercises (~50 curated) ---
        // EquipmentId: 1=Barbell, 2=Dumbbell, 3=Cable, 4=Machine, 5=Bodyweight, 6=Band, 7=Kettlebell
        // PrimaryMuscleGroupId: 1=Chest, 2=Back, 3=Shoulders, 4=Biceps, 5=Triceps, 6=Legs, 7=Glutes, 8=Core, 9=Calves
        var exercises = new List<Exercise>
        {
            // CHEST
            new() { Id = 1,  Name = "Barbell Bench Press",       PrimaryMuscleGroupId = 1, EquipmentId = 1, IsCompound = true  },
            new() { Id = 2,  Name = "Incline Barbell Press",     PrimaryMuscleGroupId = 1, EquipmentId = 1, IsCompound = true  },
            new() { Id = 3,  Name = "Dumbbell Bench Press",      PrimaryMuscleGroupId = 1, EquipmentId = 2, IsCompound = true  },
            new() { Id = 4,  Name = "Incline Dumbbell Press",    PrimaryMuscleGroupId = 1, EquipmentId = 2, IsCompound = true  },
            new() { Id = 5,  Name = "Cable Fly",                 PrimaryMuscleGroupId = 1, EquipmentId = 3, IsCompound = false },
            new() { Id = 6,  Name = "Dumbbell Fly",              PrimaryMuscleGroupId = 1, EquipmentId = 2, IsCompound = false },
            new() { Id = 7,  Name = "Push-Up",                   PrimaryMuscleGroupId = 1, EquipmentId = 5, IsCompound = true  },
            new() { Id = 8,  Name = "Dips",                      PrimaryMuscleGroupId = 1, EquipmentId = 5, IsCompound = true  },
            // BACK
            new() { Id = 9,  Name = "Barbell Deadlift",          PrimaryMuscleGroupId = 2, EquipmentId = 1, IsCompound = true  },
            new() { Id = 10, Name = "Pull-Up",                   PrimaryMuscleGroupId = 2, EquipmentId = 5, IsCompound = true  },
            new() { Id = 11, Name = "Lat Pulldown",              PrimaryMuscleGroupId = 2, EquipmentId = 3, IsCompound = true  },
            new() { Id = 12, Name = "Seated Cable Row",          PrimaryMuscleGroupId = 2, EquipmentId = 3, IsCompound = true  },
            new() { Id = 13, Name = "Barbell Row",               PrimaryMuscleGroupId = 2, EquipmentId = 1, IsCompound = true  },
            new() { Id = 14, Name = "Dumbbell Row",              PrimaryMuscleGroupId = 2, EquipmentId = 2, IsCompound = true  },
            new() { Id = 15, Name = "T-Bar Row",                 PrimaryMuscleGroupId = 2, EquipmentId = 1, IsCompound = true  },
            new() { Id = 16, Name = "Face Pull",                 PrimaryMuscleGroupId = 2, EquipmentId = 3, IsCompound = false },
            // SHOULDERS
            new() { Id = 17, Name = "Barbell Overhead Press",    PrimaryMuscleGroupId = 3, EquipmentId = 1, IsCompound = true  },
            new() { Id = 18, Name = "Dumbbell Shoulder Press",   PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = true  },
            new() { Id = 19, Name = "Lateral Raise",             PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = false },
            new() { Id = 20, Name = "Cable Lateral Raise",       PrimaryMuscleGroupId = 3, EquipmentId = 3, IsCompound = false },
            new() { Id = 21, Name = "Rear Delt Fly",             PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = false },
            new() { Id = 22, Name = "Arnold Press",              PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = false },
            // BICEPS
            new() { Id = 23, Name = "Barbell Curl",              PrimaryMuscleGroupId = 4, EquipmentId = 1, IsCompound = false },
            new() { Id = 24, Name = "Dumbbell Curl",             PrimaryMuscleGroupId = 4, EquipmentId = 2, IsCompound = false },
            new() { Id = 25, Name = "Hammer Curl",               PrimaryMuscleGroupId = 4, EquipmentId = 2, IsCompound = false },
            new() { Id = 26, Name = "Incline Dumbbell Curl",     PrimaryMuscleGroupId = 4, EquipmentId = 2, IsCompound = false },
            new() { Id = 27, Name = "Cable Curl",                PrimaryMuscleGroupId = 4, EquipmentId = 3, IsCompound = false },
            new() { Id = 28, Name = "Preacher Curl",             PrimaryMuscleGroupId = 4, EquipmentId = 4, IsCompound = false },
            // TRICEPS
            new() { Id = 29, Name = "Tricep Pushdown",           PrimaryMuscleGroupId = 5, EquipmentId = 3, IsCompound = false },
            new() { Id = 30, Name = "Overhead Tricep Extension", PrimaryMuscleGroupId = 5, EquipmentId = 2, IsCompound = false },
            new() { Id = 31, Name = "Skull Crusher",             PrimaryMuscleGroupId = 5, EquipmentId = 1, IsCompound = false },
            new() { Id = 32, Name = "Close-Grip Bench Press",    PrimaryMuscleGroupId = 5, EquipmentId = 1, IsCompound = true  },
            new() { Id = 33, Name = "Tricep Kickback",           PrimaryMuscleGroupId = 5, EquipmentId = 2, IsCompound = false },
            // LEGS
            new() { Id = 34, Name = "Barbell Back Squat",        PrimaryMuscleGroupId = 6, EquipmentId = 1, IsCompound = true  },
            new() { Id = 35, Name = "Front Squat",               PrimaryMuscleGroupId = 6, EquipmentId = 1, IsCompound = true  },
            new() { Id = 36, Name = "Leg Press",                 PrimaryMuscleGroupId = 6, EquipmentId = 4, IsCompound = true  },
            new() { Id = 37, Name = "Hack Squat",                PrimaryMuscleGroupId = 6, EquipmentId = 4, IsCompound = true  },
            new() { Id = 38, Name = "Leg Extension",             PrimaryMuscleGroupId = 6, EquipmentId = 4, IsCompound = false },
            new() { Id = 39, Name = "Leg Curl",                  PrimaryMuscleGroupId = 6, EquipmentId = 4, IsCompound = false },
            new() { Id = 40, Name = "Romanian Deadlift",         PrimaryMuscleGroupId = 6, EquipmentId = 1, IsCompound = true  },
            new() { Id = 41, Name = "Walking Lunge",             PrimaryMuscleGroupId = 6, EquipmentId = 2, IsCompound = true  },
            new() { Id = 42, Name = "Bulgarian Split Squat",     PrimaryMuscleGroupId = 6, EquipmentId = 2, IsCompound = true  },
            // GLUTES
            new() { Id = 43, Name = "Hip Thrust",                PrimaryMuscleGroupId = 7, EquipmentId = 1, IsCompound = true  },
            new() { Id = 44, Name = "Cable Kickback",            PrimaryMuscleGroupId = 7, EquipmentId = 3, IsCompound = false },
            new() { Id = 45, Name = "Sumo Deadlift",             PrimaryMuscleGroupId = 7, EquipmentId = 1, IsCompound = true  },
            // CORE
            new() { Id = 46, Name = "Plank",                     PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false },
            new() { Id = 47, Name = "Cable Crunch",              PrimaryMuscleGroupId = 8, EquipmentId = 3, IsCompound = false },
            new() { Id = 48, Name = "Hanging Leg Raise",         PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false },
            new() { Id = 49, Name = "Ab Wheel Rollout",          PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false },
            // CALVES
            new() { Id = 50, Name = "Standing Calf Raise",       PrimaryMuscleGroupId = 9, EquipmentId = 4, IsCompound = false },
            new() { Id = 51, Name = "Seated Calf Raise",         PrimaryMuscleGroupId = 9, EquipmentId = 4, IsCompound = false },
        };
        await db.InsertAllAsync(exercises);

        // --- Secondary muscle mappings ---
        var muscleLinks = new List<ExerciseMuscleGroup>
        {
            // Bench Press → Triceps (secondary), Shoulders (secondary)
            new() { ExerciseId = 1,  MuscleGroupId = 5, Role = MuscleRole.Secondary },
            new() { ExerciseId = 1,  MuscleGroupId = 3, Role = MuscleRole.Secondary },
            new() { ExerciseId = 2,  MuscleGroupId = 5, Role = MuscleRole.Secondary },
            new() { ExerciseId = 2,  MuscleGroupId = 3, Role = MuscleRole.Secondary },
            new() { ExerciseId = 3,  MuscleGroupId = 5, Role = MuscleRole.Secondary },
            new() { ExerciseId = 4,  MuscleGroupId = 5, Role = MuscleRole.Secondary },
            new() { ExerciseId = 8,  MuscleGroupId = 5, Role = MuscleRole.Secondary },
            // Deadlift → Glutes, Legs, Core
            new() { ExerciseId = 9,  MuscleGroupId = 7, Role = MuscleRole.Secondary },
            new() { ExerciseId = 9,  MuscleGroupId = 6, Role = MuscleRole.Secondary },
            new() { ExerciseId = 9,  MuscleGroupId = 8, Role = MuscleRole.Stabilizer },
            // Pull-Up / Lat Pulldown → Biceps
            new() { ExerciseId = 10, MuscleGroupId = 4, Role = MuscleRole.Secondary },
            new() { ExerciseId = 11, MuscleGroupId = 4, Role = MuscleRole.Secondary },
            new() { ExerciseId = 12, MuscleGroupId = 4, Role = MuscleRole.Secondary },
            new() { ExerciseId = 13, MuscleGroupId = 4, Role = MuscleRole.Secondary },
            new() { ExerciseId = 14, MuscleGroupId = 4, Role = MuscleRole.Secondary },
            // OHP → Triceps
            new() { ExerciseId = 17, MuscleGroupId = 5, Role = MuscleRole.Secondary },
            new() { ExerciseId = 18, MuscleGroupId = 5, Role = MuscleRole.Secondary },
            // Close-Grip Bench → Chest
            new() { ExerciseId = 32, MuscleGroupId = 1, Role = MuscleRole.Secondary },
            // Squat → Glutes, Core
            new() { ExerciseId = 34, MuscleGroupId = 7, Role = MuscleRole.Secondary },
            new() { ExerciseId = 34, MuscleGroupId = 8, Role = MuscleRole.Stabilizer },
            new() { ExerciseId = 35, MuscleGroupId = 7, Role = MuscleRole.Secondary },
            new() { ExerciseId = 35, MuscleGroupId = 8, Role = MuscleRole.Stabilizer },
            new() { ExerciseId = 36, MuscleGroupId = 7, Role = MuscleRole.Secondary },
            // RDL → Glutes
            new() { ExerciseId = 40, MuscleGroupId = 7, Role = MuscleRole.Secondary },
            new() { ExerciseId = 41, MuscleGroupId = 7, Role = MuscleRole.Secondary },
            new() { ExerciseId = 42, MuscleGroupId = 7, Role = MuscleRole.Secondary },
            // Hip Thrust → Legs
            new() { ExerciseId = 43, MuscleGroupId = 6, Role = MuscleRole.Secondary },
            new() { ExerciseId = 45, MuscleGroupId = 6, Role = MuscleRole.Secondary },
        };
        await db.InsertAllAsync(muscleLinks);
    }

    // ----------------------------------------------------------------
    // Public query helpers (used by services)
    // ----------------------------------------------------------------

    public async Task<List<T>> GetAllAsync<T>() where T : new()
    {
        var db = await GetConnectionAsync();
        return await db.Table<T>().ToListAsync();
    }

    public async Task<T?> GetByIdAsync<T>(int id) where T : new()
    {
        var db = await GetConnectionAsync();
        return await db.FindAsync<T>(id);
    }

    public async Task<int> InsertAsync<T>(T item)
    {
        var db = await GetConnectionAsync();
        return await db.InsertAsync(item);
    }

    public async Task<int> UpdateAsync<T>(T item)
    {
        var db = await GetConnectionAsync();
        return await db.UpdateAsync(item);
    }

    public async Task<int> DeleteAsync<T>(T item)
    {
        var db = await GetConnectionAsync();
        return await db.DeleteAsync(item);
    }

    public async Task<List<T>> QueryAsync<T>(string sql, params object[] args) where T : new()
    {
        var db = await GetConnectionAsync();
        return await db.QueryAsync<T>(sql, args);
    }

    public async Task<SQLiteAsyncConnection> GetRawConnectionAsync() =>
        await GetConnectionAsync();
}
