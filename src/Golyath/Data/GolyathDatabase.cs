using Golyath.Models;
using SQLite;

namespace Golyath.Data;

public class GolyathDatabase
{
    private const string DatabaseFilename = "golyath.db3";
    private const string SeedAssetName = "seed.db";
    private const string SeedVersionKey = "SeedVersion";
    private const int CurrentSeedVersion = 2;

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

        // On upgrade, wipe existing reference data to avoid PK conflicts
        if (installedVersion > 0)
        {
            await db.DeleteAllAsync<ExerciseMuscleGroup>();
            await db.DeleteAllAsync<Exercise>();
            await db.DeleteAllAsync<Equipment>();
            await db.DeleteAllAsync<MuscleGroup>();
        }
        else
        {
            // Fresh install – only seed if truly empty
            int muscleGroupCount = await db.Table<MuscleGroup>().CountAsync();
            if (muscleGroupCount > 0)
            {
                Preferences.Set(SeedVersionKey, CurrentSeedVersion);
                return;
            }
        }

        await InsertReferenceDataAsync(db);
        Preferences.Set(SeedVersionKey, CurrentSeedVersion);
    }

    private static async Task InsertReferenceDataAsync(SQLiteAsyncConnection db)
    {
        // EquipmentId: 1=Barbell, 2=Dumbbell, 3=Cable, 4=Machine, 5=Bodyweight, 6=Band, 7=Kettlebell
        // PrimaryMuscleGroupId: 1=Chest, 2=Back, 3=Shoulders, 4=Biceps, 5=Triceps, 6=Legs, 7=Glutes, 8=Core, 9=Calves

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

        // --- 100 Curated Exercises with real images ---
        var exercises = new List<Exercise>
        {
            // ── CHEST ── Horizontal Push ──────────────────────────────────────────────
            new() { Id =  1, Name = "Barbell Bench Press",                     PrimaryMuscleGroupId = 1, EquipmentId = 1, IsCompound = true,  ImagePath = "Barbell_Bench_Press_-_Medium_Grip" },
            new() { Id =  2, Name = "Dumbbell Bench Press",                    PrimaryMuscleGroupId = 1, EquipmentId = 2, IsCompound = true,  ImagePath = "Dumbbell_Bench_Press" },
            new() { Id =  3, Name = "Machine Bench Press",                     PrimaryMuscleGroupId = 1, EquipmentId = 4, IsCompound = true,  ImagePath = "Machine_Bench_Press" },
            new() { Id =  4, Name = "Wide-Grip Barbell Bench Press",           PrimaryMuscleGroupId = 1, EquipmentId = 1, IsCompound = true,  ImagePath = "Wide-Grip_Barbell_Bench_Press" },
            // ── CHEST ── Incline Push ─────────────────────────────────────────────────
            new() { Id =  5, Name = "Barbell Incline Bench Press",             PrimaryMuscleGroupId = 1, EquipmentId = 1, IsCompound = true,  ImagePath = "Barbell_Incline_Bench_Press_-_Medium_Grip" },
            new() { Id =  6, Name = "Incline Dumbbell Press",                  PrimaryMuscleGroupId = 1, EquipmentId = 2, IsCompound = true,  ImagePath = "Incline_Dumbbell_Press" },
            new() { Id =  7, Name = "Leverage Incline Chest Press",            PrimaryMuscleGroupId = 1, EquipmentId = 4, IsCompound = true,  ImagePath = "Leverage_Incline_Chest_Press" },
            // ── CHEST ── Decline Push ─────────────────────────────────────────────────
            new() { Id =  8, Name = "Decline Barbell Bench Press",             PrimaryMuscleGroupId = 1, EquipmentId = 1, IsCompound = true,  ImagePath = "Decline_Barbell_Bench_Press" },
            new() { Id =  9, Name = "Decline Dumbbell Bench Press",            PrimaryMuscleGroupId = 1, EquipmentId = 2, IsCompound = true,  ImagePath = "Decline_Dumbbell_Bench_Press" },
            // ── CHEST ── Isolation / Fly ──────────────────────────────────────────────
            new() { Id = 10, Name = "Dumbbell Flyes",                          PrimaryMuscleGroupId = 1, EquipmentId = 2, IsCompound = false, ImagePath = "Dumbbell_Flyes" },
            new() { Id = 11, Name = "Incline Dumbbell Flyes",                  PrimaryMuscleGroupId = 1, EquipmentId = 2, IsCompound = false, ImagePath = "Incline_Dumbbell_Flyes" },
            new() { Id = 12, Name = "Cable Crossover",                         PrimaryMuscleGroupId = 1, EquipmentId = 3, IsCompound = false, ImagePath = "Cable_Crossover" },
            new() { Id = 13, Name = "Cable Chest Press",                       PrimaryMuscleGroupId = 1, EquipmentId = 3, IsCompound = true,  ImagePath = "Cable_Chest_Press" },
            // ── CHEST ── Bodyweight ───────────────────────────────────────────────────
            new() { Id = 14, Name = "Push-Ups",                                PrimaryMuscleGroupId = 1, EquipmentId = 5, IsCompound = true,  ImagePath = "Pushups" },
            new() { Id = 15, Name = "Dips (Chest Version)",                    PrimaryMuscleGroupId = 1, EquipmentId = 5, IsCompound = true,  ImagePath = "Dips_-_Chest_Version" },
            new() { Id = 16, Name = "Close-Grip Barbell Bench Press",          PrimaryMuscleGroupId = 5, EquipmentId = 1, IsCompound = true,  ImagePath = "Close-Grip_Barbell_Bench_Press" },
            // ── BACK ── Vertical Pull ─────────────────────────────────────────────────
            new() { Id = 17, Name = "Pull-Ups",                                PrimaryMuscleGroupId = 2, EquipmentId = 5, IsCompound = true,  ImagePath = "Pullups" },
            new() { Id = 18, Name = "Chin-Ups",                                PrimaryMuscleGroupId = 2, EquipmentId = 5, IsCompound = true,  ImagePath = "Chin-Up" },
            new() { Id = 19, Name = "Wide-Grip Lat Pulldown",                  PrimaryMuscleGroupId = 2, EquipmentId = 3, IsCompound = true,  ImagePath = "Wide-Grip_Lat_Pulldown" },
            new() { Id = 20, Name = "Close-Grip Lat Pulldown",                 PrimaryMuscleGroupId = 2, EquipmentId = 3, IsCompound = true,  ImagePath = "Close-Grip_Front_Lat_Pulldown" },
            new() { Id = 21, Name = "V-Bar Pulldown",                          PrimaryMuscleGroupId = 2, EquipmentId = 3, IsCompound = true,  ImagePath = "V-Bar_Pulldown" },
            new() { Id = 22, Name = "Underhand Cable Pulldown",                PrimaryMuscleGroupId = 2, EquipmentId = 3, IsCompound = true,  ImagePath = "Underhand_Cable_Pulldowns" },
            new() { Id = 23, Name = "Weighted Pull-Ups",                       PrimaryMuscleGroupId = 2, EquipmentId = 5, IsCompound = true,  ImagePath = "Weighted_Pull_Ups" },
            // ── BACK ── Horizontal Pull ───────────────────────────────────────────────
            new() { Id = 24, Name = "Bent Over Barbell Row",                   PrimaryMuscleGroupId = 2, EquipmentId = 1, IsCompound = true,  ImagePath = "Bent_Over_Barbell_Row" },
            new() { Id = 25, Name = "Seated Cable Rows",                       PrimaryMuscleGroupId = 2, EquipmentId = 3, IsCompound = true,  ImagePath = "Seated_Cable_Rows" },
            new() { Id = 26, Name = "One-Arm Dumbbell Row",                    PrimaryMuscleGroupId = 2, EquipmentId = 2, IsCompound = true,  ImagePath = "One-Arm_Dumbbell_Row" },
            new() { Id = 27, Name = "T-Bar Row",                               PrimaryMuscleGroupId = 2, EquipmentId = 1, IsCompound = true,  ImagePath = "T-Bar_Row_with_Handle" },
            new() { Id = 28, Name = "Inverted Row",                            PrimaryMuscleGroupId = 2, EquipmentId = 5, IsCompound = true,  ImagePath = "Inverted_Row" },
            new() { Id = 29, Name = "Elevated Cable Rows",                     PrimaryMuscleGroupId = 2, EquipmentId = 3, IsCompound = true,  ImagePath = "Elevated_Cable_Rows" },
            // ── BACK ── Posterior Chain ───────────────────────────────────────────────
            new() { Id = 30, Name = "Barbell Deadlift",                        PrimaryMuscleGroupId = 2, EquipmentId = 1, IsCompound = true,  ImagePath = "Barbell_Deadlift" },
            new() { Id = 31, Name = "Trap Bar Deadlift",                       PrimaryMuscleGroupId = 6, EquipmentId = 5, IsCompound = true,  ImagePath = "Trap_Bar_Deadlift" },
            new() { Id = 32, Name = "Hyperextensions",                         PrimaryMuscleGroupId = 2, EquipmentId = 5, IsCompound = false, ImagePath = "Hyperextensions_Back_Extensions" },
            // ── SHOULDERS ── Anterior Delt / Press ───────────────────────────────────
            new() { Id = 33, Name = "Barbell Shoulder Press",                  PrimaryMuscleGroupId = 3, EquipmentId = 1, IsCompound = true,  ImagePath = "Barbell_Shoulder_Press" },
            new() { Id = 34, Name = "Seated Barbell Military Press",           PrimaryMuscleGroupId = 3, EquipmentId = 1, IsCompound = true,  ImagePath = "Seated_Barbell_Military_Press" },
            new() { Id = 35, Name = "Seated Dumbbell Press",                   PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = true,  ImagePath = "Seated_Dumbbell_Press" },
            new() { Id = 36, Name = "Machine Shoulder Press",                  PrimaryMuscleGroupId = 3, EquipmentId = 4, IsCompound = true,  ImagePath = "Machine_Shoulder_Military_Press" },
            new() { Id = 37, Name = "Arnold Dumbbell Press",                   PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = true,  ImagePath = "Arnold_Dumbbell_Press" },
            new() { Id = 38, Name = "Front Dumbbell Raise",                    PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = false, ImagePath = "Front_Dumbbell_Raise" },
            new() { Id = 39, Name = "Front Cable Raise",                       PrimaryMuscleGroupId = 3, EquipmentId = 3, IsCompound = false, ImagePath = "Front_Cable_Raise" },
            // ── SHOULDERS ── Lateral Delt ─────────────────────────────────────────────
            new() { Id = 40, Name = "Side Lateral Raise",                      PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = false, ImagePath = "Side_Lateral_Raise" },
            new() { Id = 41, Name = "Cable Lateral Raise",                     PrimaryMuscleGroupId = 3, EquipmentId = 3, IsCompound = false, ImagePath = "Cable_Seated_Lateral_Raise" },
            new() { Id = 42, Name = "Seated Side Lateral Raise",               PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = false, ImagePath = "Seated_Side_Lateral_Raise" },
            new() { Id = 43, Name = "Upright Barbell Row",                     PrimaryMuscleGroupId = 3, EquipmentId = 1, IsCompound = true,  ImagePath = "Upright_Barbell_Row" },
            // ── SHOULDERS ── Posterior Delt ───────────────────────────────────────────
            new() { Id = 44, Name = "Face Pull",                               PrimaryMuscleGroupId = 3, EquipmentId = 3, IsCompound = false, ImagePath = "Face_Pull" },
            new() { Id = 45, Name = "Rear Delt Dumbbell Raise",                PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = false, ImagePath = "Bent_Over_Dumbbell_Rear_Delt_Raise_With_Head_On_Bench" },
            new() { Id = 46, Name = "Cable Rear Delt Fly",                     PrimaryMuscleGroupId = 3, EquipmentId = 3, IsCompound = false, ImagePath = "Cable_Rear_Delt_Fly" },
            new() { Id = 47, Name = "Reverse Machine Flyes",                   PrimaryMuscleGroupId = 3, EquipmentId = 4, IsCompound = false, ImagePath = "Reverse_Machine_Flyes" },
            new() { Id = 48, Name = "Seated Bent-Over Rear Delt Raise",        PrimaryMuscleGroupId = 3, EquipmentId = 2, IsCompound = false, ImagePath = "Seated_Bent-Over_Rear_Delt_Raise" },
            // ── BICEPS ────────────────────────────────────────────────────────────────
            new() { Id = 49, Name = "Barbell Curl",                            PrimaryMuscleGroupId = 4, EquipmentId = 1, IsCompound = false, ImagePath = "Barbell_Curl" },
            new() { Id = 50, Name = "Dumbbell Bicep Curl",                     PrimaryMuscleGroupId = 4, EquipmentId = 2, IsCompound = false, ImagePath = "Dumbbell_Bicep_Curl" },
            new() { Id = 51, Name = "Hammer Curls",                            PrimaryMuscleGroupId = 4, EquipmentId = 2, IsCompound = false, ImagePath = "Hammer_Curls" },
            new() { Id = 52, Name = "Incline Dumbbell Curl",                   PrimaryMuscleGroupId = 4, EquipmentId = 2, IsCompound = false, ImagePath = "Incline_Dumbbell_Curl" },
            new() { Id = 53, Name = "Concentration Curls",                     PrimaryMuscleGroupId = 4, EquipmentId = 2, IsCompound = false, ImagePath = "Concentration_Curls" },
            new() { Id = 54, Name = "Preacher Curl",                           PrimaryMuscleGroupId = 4, EquipmentId = 1, IsCompound = false, ImagePath = "Preacher_Curl" },
            new() { Id = 55, Name = "EZ-Bar Curl",                             PrimaryMuscleGroupId = 4, EquipmentId = 1, IsCompound = false, ImagePath = "EZ-Bar_Curl" },
            new() { Id = 56, Name = "Cable Preacher Curl",                     PrimaryMuscleGroupId = 4, EquipmentId = 3, IsCompound = false, ImagePath = "Cable_Preacher_Curl" },
            new() { Id = 57, Name = "High Cable Curls",                        PrimaryMuscleGroupId = 4, EquipmentId = 3, IsCompound = false, ImagePath = "High_Cable_Curls" },
            // ── TRICEPS ───────────────────────────────────────────────────────────────
            new() { Id = 58, Name = "Triceps Pushdown",                        PrimaryMuscleGroupId = 5, EquipmentId = 3, IsCompound = false, ImagePath = "Triceps_Pushdown" },
            new() { Id = 59, Name = "Triceps Pushdown - Rope",                 PrimaryMuscleGroupId = 5, EquipmentId = 3, IsCompound = false, ImagePath = "Triceps_Pushdown_-_Rope_Attachment" },
            new() { Id = 60, Name = "EZ-Bar Skullcrusher",                     PrimaryMuscleGroupId = 5, EquipmentId = 1, IsCompound = false, ImagePath = "EZ-Bar_Skullcrusher" },
            new() { Id = 61, Name = "Dips (Triceps Version)",                  PrimaryMuscleGroupId = 5, EquipmentId = 5, IsCompound = true,  ImagePath = "Dips_-_Triceps_Version" },
            new() { Id = 62, Name = "Bench Dips",                              PrimaryMuscleGroupId = 5, EquipmentId = 5, IsCompound = true,  ImagePath = "Bench_Dips" },
            new() { Id = 63, Name = "Cable Rope Overhead Triceps Extension",   PrimaryMuscleGroupId = 5, EquipmentId = 3, IsCompound = false, ImagePath = "Cable_Rope_Overhead_Triceps_Extension" },
            new() { Id = 64, Name = "Lying Dumbbell Tricep Extension",         PrimaryMuscleGroupId = 5, EquipmentId = 2, IsCompound = false, ImagePath = "Lying_Dumbbell_Tricep_Extension" },
            new() { Id = 65, Name = "Machine Triceps Extension",               PrimaryMuscleGroupId = 5, EquipmentId = 4, IsCompound = false, ImagePath = "Machine_Triceps_Extension" },
            new() { Id = 66, Name = "Tricep Dumbbell Kickback",                PrimaryMuscleGroupId = 5, EquipmentId = 2, IsCompound = false, ImagePath = "Tricep_Dumbbell_Kickback" },
            // ── LEGS ── Quad Dominant ─────────────────────────────────────────────────
            new() { Id = 67, Name = "Barbell Squat",                           PrimaryMuscleGroupId = 6, EquipmentId = 1, IsCompound = true,  ImagePath = "Barbell_Squat" },
            new() { Id = 68, Name = "Front Barbell Squat",                     PrimaryMuscleGroupId = 6, EquipmentId = 1, IsCompound = true,  ImagePath = "Front_Barbell_Squat" },
            new() { Id = 69, Name = "Leg Press",                               PrimaryMuscleGroupId = 6, EquipmentId = 4, IsCompound = true,  ImagePath = "Leg_Press" },
            new() { Id = 70, Name = "Hack Squat",                              PrimaryMuscleGroupId = 6, EquipmentId = 4, IsCompound = true,  ImagePath = "Hack_Squat" },
            new() { Id = 71, Name = "Leg Extensions",                          PrimaryMuscleGroupId = 6, EquipmentId = 4, IsCompound = false, ImagePath = "Leg_Extensions" },
            new() { Id = 72, Name = "Dumbbell Squat",                          PrimaryMuscleGroupId = 6, EquipmentId = 2, IsCompound = true,  ImagePath = "Dumbbell_Squat" },
            new() { Id = 73, Name = "Goblet Squat",                            PrimaryMuscleGroupId = 6, EquipmentId = 7, IsCompound = true,  ImagePath = "Goblet_Squat" },
            new() { Id = 74, Name = "Barbell Lunge",                           PrimaryMuscleGroupId = 6, EquipmentId = 1, IsCompound = true,  ImagePath = "Barbell_Lunge" },
            new() { Id = 75, Name = "Dumbbell Lunges",                         PrimaryMuscleGroupId = 6, EquipmentId = 2, IsCompound = true,  ImagePath = "Dumbbell_Lunges" },
            new() { Id = 76, Name = "Barbell Walking Lunge",                   PrimaryMuscleGroupId = 6, EquipmentId = 1, IsCompound = true,  ImagePath = "Barbell_Walking_Lunge" },
            new() { Id = 77, Name = "Dumbbell Step Ups",                       PrimaryMuscleGroupId = 6, EquipmentId = 2, IsCompound = true,  ImagePath = "Dumbbell_Step_Ups" },
            // ── LEGS ── Hamstring Dominant ────────────────────────────────────────────
            new() { Id = 78, Name = "Romanian Deadlift",                       PrimaryMuscleGroupId = 6, EquipmentId = 1, IsCompound = true,  ImagePath = "Romanian_Deadlift" },
            new() { Id = 79, Name = "Stiff-Legged Barbell Deadlift",           PrimaryMuscleGroupId = 6, EquipmentId = 1, IsCompound = true,  ImagePath = "Stiff-Legged_Barbell_Deadlift" },
            new() { Id = 80, Name = "Good Morning",                            PrimaryMuscleGroupId = 6, EquipmentId = 1, IsCompound = true,  ImagePath = "Good_Morning" },
            new() { Id = 81, Name = "Lying Leg Curls",                         PrimaryMuscleGroupId = 6, EquipmentId = 4, IsCompound = false, ImagePath = "Lying_Leg_Curls" },
            new() { Id = 82, Name = "Seated Leg Curl",                         PrimaryMuscleGroupId = 6, EquipmentId = 4, IsCompound = false, ImagePath = "Seated_Leg_Curl" },
            // ── GLUTES ────────────────────────────────────────────────────────────────
            new() { Id = 83, Name = "Barbell Hip Thrust",                      PrimaryMuscleGroupId = 7, EquipmentId = 1, IsCompound = true,  ImagePath = "Barbell_Hip_Thrust" },
            new() { Id = 84, Name = "Barbell Glute Bridge",                    PrimaryMuscleGroupId = 7, EquipmentId = 1, IsCompound = true,  ImagePath = "Barbell_Glute_Bridge" },
            new() { Id = 85, Name = "Glute Kickback",                          PrimaryMuscleGroupId = 7, EquipmentId = 5, IsCompound = false, ImagePath = "Glute_Kickback" },
            new() { Id = 86, Name = "Sumo Deadlift",                           PrimaryMuscleGroupId = 7, EquipmentId = 1, IsCompound = true,  ImagePath = "Sumo_Deadlift" },
            new() { Id = 87, Name = "Flutter Kicks",                           PrimaryMuscleGroupId = 7, EquipmentId = 5, IsCompound = false, ImagePath = "Flutter_Kicks" },
            // ── CALVES ────────────────────────────────────────────────────────────────
            new() { Id = 88, Name = "Standing Calf Raises",                    PrimaryMuscleGroupId = 9, EquipmentId = 4, IsCompound = false, ImagePath = "Standing_Calf_Raises" },
            new() { Id = 89, Name = "Seated Calf Raise",                       PrimaryMuscleGroupId = 9, EquipmentId = 4, IsCompound = false, ImagePath = "Seated_Calf_Raise" },
            new() { Id = 90, Name = "Calf Press on Leg Press Machine",         PrimaryMuscleGroupId = 9, EquipmentId = 4, IsCompound = false, ImagePath = "Calf_Press_On_The_Leg_Press_Machine" },
            // ── CORE ──────────────────────────────────────────────────────────────────
            new() { Id = 91, Name = "Crunches",                                PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false, ImagePath = "Crunches" },
            new() { Id = 92, Name = "Cable Crunch",                            PrimaryMuscleGroupId = 8, EquipmentId = 3, IsCompound = false, ImagePath = "Cable_Crunch" },
            new() { Id = 93, Name = "Hanging Leg Raise",                       PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false, ImagePath = "Hanging_Leg_Raise" },
            new() { Id = 94, Name = "Plank",                                   PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false, ImagePath = "Plank" },
            new() { Id = 95, Name = "Russian Twist",                           PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false, ImagePath = "Russian_Twist" },
            new() { Id = 96, Name = "Decline Crunch",                          PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false, ImagePath = "Decline_Crunch" },
            new() { Id = 97, Name = "Leg Pull-In",                             PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false, ImagePath = "Leg_Pull-In" },
            new() { Id = 98, Name = "Ab Roller",                               PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = true,  ImagePath = "Ab_Roller" },
            new() { Id = 99, Name = "Side Bridge (Side Plank)",                PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false, ImagePath = "Side_Bridge" },
            new() { Id = 100, Name = "Flutter Kicks",                          PrimaryMuscleGroupId = 8, EquipmentId = 5, IsCompound = false, ImagePath = "Flutter_Kicks" },
        };

        // Remove duplicate Flutter Kicks (it was both glutes and core — keep core version at 100, remove 87)
        exercises.RemoveAll(e => e.Id == 87);

        await db.InsertAllAsync(exercises);

        // --- Secondary muscle mappings ---
        // MuscleGroupId: 1=Chest, 2=Back, 3=Shoulders, 4=Biceps, 5=Triceps, 6=Legs, 7=Glutes, 8=Core, 9=Calves
        var muscleLinks = new List<ExerciseMuscleGroup>
        {
            // ── CHEST ─────────────────────────────────────────────────────────────────
            new() { ExerciseId =  1, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Bench → Shoulders
            new() { ExerciseId =  1, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Bench → Triceps
            new() { ExerciseId =  2, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // DB Bench → Shoulders
            new() { ExerciseId =  2, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // DB Bench → Triceps
            new() { ExerciseId =  3, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Machine Bench → Shoulders
            new() { ExerciseId =  3, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Machine Bench → Triceps
            new() { ExerciseId =  4, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Wide Bench → Shoulders
            new() { ExerciseId =  4, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Wide Bench → Triceps
            new() { ExerciseId =  5, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Incline BB → Shoulders
            new() { ExerciseId =  5, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Incline BB → Triceps
            new() { ExerciseId =  6, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Incline DB → Shoulders
            new() { ExerciseId =  6, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Incline DB → Triceps
            new() { ExerciseId =  7, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Leverage Incline → Shoulders
            new() { ExerciseId =  7, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Leverage Incline → Triceps
            new() { ExerciseId =  8, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Decline BB → Shoulders
            new() { ExerciseId =  8, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Decline BB → Triceps
            new() { ExerciseId =  9, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Decline DB → Shoulders
            new() { ExerciseId =  9, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Decline DB → Triceps
            new() { ExerciseId = 12, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Cable Crossover → Shoulders
            new() { ExerciseId = 13, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Cable Chest Press → Shoulders
            new() { ExerciseId = 13, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Cable Chest Press → Triceps
            new() { ExerciseId = 14, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Push-Ups → Shoulders
            new() { ExerciseId = 14, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Push-Ups → Triceps
            new() { ExerciseId = 15, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Dips Chest → Shoulders
            new() { ExerciseId = 15, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Dips Chest → Triceps
            new() { ExerciseId = 16, MuscleGroupId = 1, Role = MuscleRole.Secondary },  // Close-Grip Bench → Chest
            new() { ExerciseId = 16, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Close-Grip Bench → Shoulders
            // ── BACK ──────────────────────────────────────────────────────────────────
            new() { ExerciseId = 17, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // Pull-Ups → Biceps
            new() { ExerciseId = 18, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // Chin-Ups → Biceps
            new() { ExerciseId = 19, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // Wide Pulldown → Biceps
            new() { ExerciseId = 20, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // Close Pulldown → Biceps
            new() { ExerciseId = 21, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // V-Bar Pulldown → Biceps
            new() { ExerciseId = 22, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // Underhand Pulldown → Biceps
            new() { ExerciseId = 23, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // Weighted Pull-Ups → Biceps
            new() { ExerciseId = 24, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // BB Row → Biceps
            new() { ExerciseId = 25, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // Seated Cable Row → Biceps
            new() { ExerciseId = 26, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // DB Row → Biceps
            new() { ExerciseId = 27, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // T-Bar → Biceps
            new() { ExerciseId = 30, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // Deadlift → Glutes
            new() { ExerciseId = 30, MuscleGroupId = 6, Role = MuscleRole.Secondary },  // Deadlift → Legs
            new() { ExerciseId = 30, MuscleGroupId = 8, Role = MuscleRole.Stabilizer }, // Deadlift → Core
            new() { ExerciseId = 31, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // Trap Bar → Glutes
            new() { ExerciseId = 31, MuscleGroupId = 6, Role = MuscleRole.Secondary },  // Trap Bar → Legs
            new() { ExerciseId = 32, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // Hyperext → Glutes
            new() { ExerciseId = 32, MuscleGroupId = 6, Role = MuscleRole.Secondary },  // Hyperext → Hamstrings
            // ── SHOULDERS ─────────────────────────────────────────────────────────────
            new() { ExerciseId = 33, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // BB OHP → Triceps
            new() { ExerciseId = 33, MuscleGroupId = 1, Role = MuscleRole.Secondary },  // BB OHP → Chest
            new() { ExerciseId = 34, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Seated BB Press → Triceps
            new() { ExerciseId = 35, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Seated DB Press → Triceps
            new() { ExerciseId = 36, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Machine Press → Triceps
            new() { ExerciseId = 37, MuscleGroupId = 5, Role = MuscleRole.Secondary },  // Arnold Press → Triceps
            new() { ExerciseId = 43, MuscleGroupId = 4, Role = MuscleRole.Secondary },  // Upright Row → Biceps
            // ── BICEPS ────────────────────────────────────────────────────────────────
            // (primary-only for most curls; forearms not mapped to DB groups)
            // ── TRICEPS ───────────────────────────────────────────────────────────────
            new() { ExerciseId = 61, MuscleGroupId = 1, Role = MuscleRole.Secondary },  // Tricep Dips → Chest
            new() { ExerciseId = 61, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Tricep Dips → Shoulders
            new() { ExerciseId = 62, MuscleGroupId = 1, Role = MuscleRole.Secondary },  // Bench Dips → Chest
            new() { ExerciseId = 62, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Bench Dips → Shoulders
            // ── LEGS ──────────────────────────────────────────────────────────────────
            new() { ExerciseId = 67, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // BB Squat → Glutes
            new() { ExerciseId = 67, MuscleGroupId = 9, Role = MuscleRole.Secondary },  // BB Squat → Calves
            new() { ExerciseId = 67, MuscleGroupId = 8, Role = MuscleRole.Stabilizer }, // BB Squat → Core
            new() { ExerciseId = 68, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // Front Squat → Glutes
            new() { ExerciseId = 68, MuscleGroupId = 9, Role = MuscleRole.Secondary },  // Front Squat → Calves
            new() { ExerciseId = 69, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // Leg Press → Glutes
            new() { ExerciseId = 69, MuscleGroupId = 9, Role = MuscleRole.Secondary },  // Leg Press → Calves
            new() { ExerciseId = 70, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // Hack Squat → Glutes
            new() { ExerciseId = 70, MuscleGroupId = 9, Role = MuscleRole.Secondary },  // Hack Squat → Calves
            new() { ExerciseId = 72, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // DB Squat → Glutes
            new() { ExerciseId = 73, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // Goblet Squat → Glutes
            new() { ExerciseId = 74, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // BB Lunge → Glutes
            new() { ExerciseId = 74, MuscleGroupId = 9, Role = MuscleRole.Secondary },  // BB Lunge → Calves
            new() { ExerciseId = 75, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // DB Lunges → Glutes
            new() { ExerciseId = 75, MuscleGroupId = 9, Role = MuscleRole.Secondary },  // DB Lunges → Calves
            new() { ExerciseId = 76, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // BB Walking Lunge → Glutes
            new() { ExerciseId = 76, MuscleGroupId = 9, Role = MuscleRole.Secondary },  // BB Walking Lunge → Calves
            new() { ExerciseId = 77, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // Step Ups → Glutes
            new() { ExerciseId = 77, MuscleGroupId = 9, Role = MuscleRole.Secondary },  // Step Ups → Calves
            new() { ExerciseId = 78, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // RDL → Glutes
            new() { ExerciseId = 78, MuscleGroupId = 2, Role = MuscleRole.Secondary },  // RDL → Back
            new() { ExerciseId = 79, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // Stiff-Leg DL → Glutes
            new() { ExerciseId = 79, MuscleGroupId = 2, Role = MuscleRole.Secondary },  // Stiff-Leg DL → Back
            new() { ExerciseId = 80, MuscleGroupId = 7, Role = MuscleRole.Secondary },  // Good Morning → Glutes
            new() { ExerciseId = 80, MuscleGroupId = 2, Role = MuscleRole.Secondary },  // Good Morning → Back
            // ── GLUTES ────────────────────────────────────────────────────────────────
            new() { ExerciseId = 83, MuscleGroupId = 6, Role = MuscleRole.Secondary },  // Hip Thrust → Legs
            new() { ExerciseId = 83, MuscleGroupId = 9, Role = MuscleRole.Secondary },  // Hip Thrust → Calves
            new() { ExerciseId = 84, MuscleGroupId = 6, Role = MuscleRole.Secondary },  // Glute Bridge → Legs
            new() { ExerciseId = 85, MuscleGroupId = 6, Role = MuscleRole.Secondary },  // Kickback → Legs
            new() { ExerciseId = 86, MuscleGroupId = 6, Role = MuscleRole.Secondary },  // Sumo DL → Legs
            new() { ExerciseId = 86, MuscleGroupId = 2, Role = MuscleRole.Secondary },  // Sumo DL → Back
            // ── CORE ──────────────────────────────────────────────────────────────────
            new() { ExerciseId = 95, MuscleGroupId = 2, Role = MuscleRole.Secondary },  // Russian Twist → Back
            new() { ExerciseId = 98, MuscleGroupId = 3, Role = MuscleRole.Secondary },  // Ab Roller → Shoulders
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
