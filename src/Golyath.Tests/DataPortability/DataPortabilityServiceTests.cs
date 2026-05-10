using System.Text.Json;
using Golyath.Application.DTOs;
using Golyath.Application.Services;
using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Core.Enums;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Golyath.Tests.DataPortability;

/// <summary>
/// Unit tests for DataPortabilityService.
/// Tests are written against the IDataPortabilityService contract and BackupDocument design.
/// All repositories are mocked; no real SQLite is involved.
/// NOTE: These tests will compile once Kaylee's DataPortabilityService + BackupDtos land.
/// </summary>
public class DataPortabilityServiceTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IExerciseRepository _exerciseRepo = Substitute.For<IExerciseRepository>();
    private readonly IWorkoutRepository _workoutRepo = Substitute.For<IWorkoutRepository>();
    private readonly IWorkoutExerciseRepository _workoutExerciseRepo = Substitute.For<IWorkoutExerciseRepository>();
    private readonly IWorkoutSetRepository _workoutSetRepo = Substitute.For<IWorkoutSetRepository>();
    private readonly IGoalRepository _goalRepo = Substitute.For<IGoalRepository>();
    private readonly ITagRepository _tagRepo = Substitute.For<ITagRepository>();
    private readonly IWorkoutTagRepository _workoutTagRepo = Substitute.For<IWorkoutTagRepository>();

    public DataPortabilityServiceTests()
    {
        // xUnit creates a new test class instance per test, so these defaults are isolated.
        SetupDefaultEmptyReturns();
    }

    private void SetupDefaultEmptyReturns()
    {
        _userRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<User>>([]));
        _exerciseRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Exercise>>([]));
        _workoutRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Workout>>([]));
        _workoutExerciseRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<WorkoutExercise>>([]));
        _workoutSetRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<WorkoutSet>>([]));
        _goalRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Goal>>([]));
        _tagRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Tag>>([]));
        _workoutTagRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<WorkoutTag>>([]));
    }

    private DataPortabilityService CreateService() =>
        new(_userRepo, _exerciseRepo, _workoutRepo, _workoutExerciseRepo,
            _workoutSetRepo, _goalRepo, _tagRepo, _workoutTagRepo);

    // ────────────────────────────────────────────────────────────────────────
    // Helpers
    // ────────────────────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static User MakeUser(string nickname = "Gabriel") => new()
    {
        Id = 1,
        Nickname = nickname,
        Birthday = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        HeightCm = 180,
        WeightKg = 80,
        Gender = Gender.Male,
        FitnessGoal = FitnessGoal.Strength,
        PreferredUnit = WeightUnit.Kg,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    private static Exercise MakeExercise(bool isCustom = false, string? externalId = "Barbell_Squat") => new()
    {
        Id = 1,
        Name = "Barbell Squat",
        IsCustom = isCustom,
        ExternalId = externalId,
        PrimaryMuscle = MuscleGroup.Quads,
        MovementType = MovementType.Legs,
        Equipment = EquipmentType.Barbell
    };

    private static Workout MakeWorkout() => new()
    {
        Id = 1,
        Name = "Leg Day",
        StartedAt = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    };

    private static Tag MakeTag(string name = "Bulking") => new() { Id = 1, Name = name };

    /// <summary>
    /// Builds a minimal valid JSON export document for use in import tests.
    /// </summary>
    private static string MakeBackupJson(
        int schemaVersion = 1,
        List<UserBackup>? users = null,
        List<ExerciseBackup>? exercises = null,
        List<WorkoutBackup>? workouts = null,
        List<WorkoutExerciseBackup>? workoutExercises = null,
        List<WorkoutSetBackup>? workoutSets = null,
        List<GoalBackup>? goals = null,
        List<TagBackup>? tags = null,
        List<WorkoutTagBackup>? workoutTags = null)
    {
        var doc = new BackupDocument
        {
            BackupSchemaVersion = schemaVersion,
            AppVersion = "1.0.0",
            ExportedAt = DateTime.UtcNow,
            Data = new BackupData
            {
                Users = users ?? [],
                Exercises = exercises ?? [],
                Workouts = workouts ?? [],
                WorkoutExercises = workoutExercises ?? [],
                WorkoutSets = workoutSets ?? [],
                Goals = goals ?? [],
                Tags = tags ?? [],
                WorkoutTags = workoutTags ?? []
            }
        };
        return JsonSerializer.Serialize(doc, CamelCase);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Export tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExportToJsonAsync_ReturnsValidJson()
    {
        var service = CreateService();

        var json = await service.ExportToJsonAsync();

        Assert.False(string.IsNullOrWhiteSpace(json));
        var doc = JsonSerializer.Deserialize<BackupDocument>(json, CamelCase);
        Assert.NotNull(doc);
        Assert.Equal(1, doc.BackupSchemaVersion);
        Assert.False(string.IsNullOrWhiteSpace(doc.AppVersion));
        Assert.True(doc.ExportedAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task ExportToJsonAsync_IncludesAllEntities()
    {
        _userRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<User>>([MakeUser()]));
        _workoutRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Workout>>([MakeWorkout()]));
        _exerciseRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Exercise>>([MakeExercise()]));
        var service = CreateService();

        var json = await service.ExportToJsonAsync();

        var doc = JsonSerializer.Deserialize<BackupDocument>(json, CamelCase);
        Assert.NotNull(doc);
        Assert.Single(doc.Data.Users);
        Assert.Single(doc.Data.Workouts);
        Assert.Single(doc.Data.Exercises);
    }

    [Fact]
    public async Task ExportToJsonAsync_IncludesBothCustomAndSeededExercises()
    {
        IReadOnlyList<Exercise> exercises =
        [
            new() { Id = 1, Name = "Barbell Squat", IsCustom = false, ExternalId = "Barbell_Squat" },
            new() { Id = 2, Name = "My Custom Exercise", IsCustom = true, ExternalId = null }
        ];
        _exerciseRepo.GetAllAsync().Returns(Task.FromResult(exercises));
        var service = CreateService();

        var json = await service.ExportToJsonAsync();

        var doc = JsonSerializer.Deserialize<BackupDocument>(json, CamelCase);
        Assert.NotNull(doc);
        Assert.Equal(2, doc.Data.Exercises.Count);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Import tests
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ImportFromJsonAsync_InvalidJson_ReturnsFalse()
    {
        var service = CreateService();

        var result = await service.ImportFromJsonAsync("not valid json");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ImportFromJsonAsync_WrongSchemaVersion_ReturnsFalse()
    {
        var json = MakeBackupJson(schemaVersion: 99);
        var service = CreateService();

        var result = await service.ImportFromJsonAsync(json);

        Assert.False(result.Success);
        Assert.Contains("version", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportFromJsonAsync_EmptyBackup_ReturnsSuccess()
    {
        var json = MakeBackupJson();
        var service = CreateService();

        var result = await service.ImportFromJsonAsync(json);

        Assert.True(result.Success);
        Assert.Equal(0, result.ItemsImported);
    }

    [Fact]
    public async Task ImportFromJsonAsync_NewTag_IsInserted()
    {
        // All existing tags already set to empty in constructor.
        var json = MakeBackupJson(tags: [new TagBackup(1, "Bulking", null)]);
        var service = CreateService();

        await service.ImportFromJsonAsync(json);

        await _tagRepo.Received(1).InsertAsync(Arg.Any<Tag>());
    }

    [Fact]
    public async Task ImportFromJsonAsync_DuplicateTag_IsSkipped()
    {
        _tagRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Tag>>([MakeTag("Bulking")]));
        var json = MakeBackupJson(tags: [new TagBackup(1, "Bulking", null)]);
        var service = CreateService();

        await service.ImportFromJsonAsync(json);

        await _tagRepo.DidNotReceive().InsertAsync(Arg.Any<Tag>());
    }

    [Fact]
    public async Task ImportFromJsonAsync_NewUser_IsInserted()
    {
        // Existing users already empty from constructor.
        _userRepo.InsertAsync(Arg.Any<User>()).Returns(callInfo =>
        {
            callInfo.Arg<User>().Id = 1; // simulate SQLite auto-increment
            return Task.FromResult(1);
        });
        var json = MakeBackupJson(users:
        [
            new UserBackup(1, "Gabriel", new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                180, 80, Gender.Male, FitnessGoal.Strength, WeightUnit.Kg,
                DateTime.UtcNow, DateTime.UtcNow)
        ]);
        var service = CreateService();

        await service.ImportFromJsonAsync(json);

        await _userRepo.Received(1).InsertAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task ImportFromJsonAsync_DuplicateUser_IsSkipped()
    {
        _userRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<User>>([MakeUser("Gabriel")]));
        var json = MakeBackupJson(users:
        [
            new UserBackup(1, "Gabriel", new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                180, 80, Gender.Male, FitnessGoal.Strength, WeightUnit.Kg,
                DateTime.UtcNow, DateTime.UtcNow)
        ]);
        var service = CreateService();

        await service.ImportFromJsonAsync(json);

        await _userRepo.DidNotReceive().InsertAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task ImportFromJsonAsync_NewCustomExercise_IsInserted()
    {
        // Existing exercises already empty from constructor.
        _exerciseRepo.InsertAsync(Arg.Any<Exercise>()).Returns(callInfo =>
        {
            callInfo.Arg<Exercise>().Id = 1;
            return Task.FromResult(1);
        });
        var json = MakeBackupJson(exercises:
        [
            new ExerciseBackup(1, "My Custom Exercise", true,
                MuscleGroup.Chest, string.Empty, MovementType.Push,
                EquipmentType.Dumbbell, null, null, string.Empty)
        ]);
        var service = CreateService();

        await service.ImportFromJsonAsync(json);

        await _exerciseRepo.Received(1).InsertAsync(Arg.Any<Exercise>());
    }

    [Fact]
    public async Task ImportFromJsonAsync_SeededExerciseRemapped_NotInserted()
    {
        // Existing DB already has this seeded exercise by ExternalId.
        _exerciseRepo.GetAllAsync().Returns(Task.FromResult<IReadOnlyList<Exercise>>(
            [MakeExercise(isCustom: false, externalId: "Barbell_Squat")]));

        // Backup contains the same seeded exercise (possibly with a different backup Id).
        var json = MakeBackupJson(exercises:
        [
            new ExerciseBackup(999, "Barbell Squat", false,
                MuscleGroup.Quads, string.Empty, MovementType.Legs,
                EquipmentType.Barbell, null, "Barbell_Squat", string.Empty)
        ]);
        var service = CreateService();

        await service.ImportFromJsonAsync(json);

        // The service should remap by ExternalId; no new row should be inserted.
        await _exerciseRepo.DidNotReceive().InsertAsync(Arg.Any<Exercise>());
    }

    [Fact]
    public async Task ImportFromJsonAsync_ReturnsCorrectItemCount()
    {
        // 1 user + 2 new tags = 3 items imported
        _userRepo.InsertAsync(Arg.Any<User>()).Returns(callInfo =>
        {
            callInfo.Arg<User>().Id = 1;
            return Task.FromResult(1);
        });
        _tagRepo.InsertAsync(Arg.Any<Tag>()).Returns(Task.FromResult(1));

        var json = MakeBackupJson(
            users:
            [
                new UserBackup(1, "Gabriel", new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    180, 80, Gender.Male, FitnessGoal.Strength, WeightUnit.Kg,
                    DateTime.UtcNow, DateTime.UtcNow)
            ],
            tags:
            [
                new TagBackup(1, "Bulking", null),
                new TagBackup(2, "Cutting", null)
            ]);
        var service = CreateService();

        var result = await service.ImportFromJsonAsync(json);

        Assert.Equal(3, result.ItemsImported);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Error handling
    // ────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ImportFromJsonAsync_RepositoryThrows_ReturnsFalse()
    {
        _userRepo.GetAllAsync().ThrowsAsync(new Exception("Simulated DB failure"));
        var json = MakeBackupJson(users:
        [
            new UserBackup(1, "Gabriel", new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                180, 80, Gender.Male, FitnessGoal.Strength, WeightUnit.Kg,
                DateTime.UtcNow, DateTime.UtcNow)
        ]);
        var service = CreateService();

        var result = await service.ImportFromJsonAsync(json);

        Assert.False(result.Success);
    }
}
