using Golyath.Core.Entities;
using Golyath.Core.Enums;
using Golyath.Infrastructure.Database.Migrations;
using Golyath.Infrastructure.Database.Repositories;
using Golyath.Tests.Helpers;
using Xunit;

namespace Golyath.Tests.Infrastructure;

/// <remarks>
/// TESTABILITY GAP — same constraint as SqliteUserRepositoryTests.
/// <c>SqliteExerciseRepository</c> must expose a constructor that accepts
/// <c>SQLiteAsyncConnection</c> directly.  See duke-testability-gap.md.
/// </remarks>
public sealed class SqliteExerciseRepositoryTests : IAsyncLifetime
{
    private TempDatabase _db = null!;
    private SqliteExerciseRepository _repository = null!;

    public async Task InitializeAsync()
    {
        _db = new TempDatabase();
        var migration = new Migration_0001_InitialSchema();
        await migration.UpAsync(_db.Connection);
        _repository = new SqliteExerciseRepository(_db.Connection);
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    // ── SearchAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_ReturnsMatchingExercises()
    {
        await _repository.InsertAsync(new Exercise
        {
            Name = "Barbell Squat",
            PrimaryMuscle = "Quads",
            MovementType = MovementType.Legs,
            EquipmentType = EquipmentType.Barbell
        });
        await _repository.InsertAsync(new Exercise
        {
            Name = "Dumbbell Press",
            PrimaryMuscle = "Chest",
            MovementType = MovementType.Push,
            EquipmentType = EquipmentType.Dumbbell
        });

        var results = await _repository.SearchAsync("squat");

        Assert.Single(results);
        Assert.Equal("Barbell Squat", results.First().Name);
    }

    [Fact]
    public async Task SearchAsync_IsCaseInsensitive()
    {
        await _repository.InsertAsync(new Exercise
        {
            Name = "Barbell Squat",
            PrimaryMuscle = "Quads",
            MovementType = MovementType.Legs,
            EquipmentType = EquipmentType.Barbell
        });

        var upper = await _repository.SearchAsync("SQUAT");
        var lower = await _repository.SearchAsync("squat");

        Assert.Single(upper);
        Assert.Single(lower);
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsAllExercises()
    {
        await _repository.InsertAsync(new Exercise { Name = "Squat", PrimaryMuscle = "Quads", MovementType = MovementType.Legs, EquipmentType = EquipmentType.Barbell });
        await _repository.InsertAsync(new Exercise { Name = "Press", PrimaryMuscle = "Chest", MovementType = MovementType.Push, EquipmentType = EquipmentType.Dumbbell });

        var results = await _repository.SearchAsync(string.Empty);

        Assert.Equal(2, results.Count());
    }

    // ── GetByMuscleGroupAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetByMuscleGroupAsync_ReturnsCorrectExercises()
    {
        await _repository.InsertAsync(new Exercise
        {
            Name = "Squat",
            PrimaryMuscle = "Quads",
            MovementType = MovementType.Legs,
            EquipmentType = EquipmentType.Barbell
        });
        await _repository.InsertAsync(new Exercise
        {
            Name = "Bench Press",
            PrimaryMuscle = "Chest",
            MovementType = MovementType.Push,
            EquipmentType = EquipmentType.Barbell
        });

        var results = await _repository.GetByMuscleGroupAsync("Quads");

        Assert.Single(results);
        Assert.Equal("Squat", results.First().Name);
    }

    [Fact]
    public async Task GetByMuscleGroupAsync_NoMatch_ReturnsEmpty()
    {
        await _repository.InsertAsync(new Exercise
        {
            Name = "Squat",
            PrimaryMuscle = "Quads",
            MovementType = MovementType.Legs,
            EquipmentType = EquipmentType.Barbell
        });

        var results = await _repository.GetByMuscleGroupAsync("Biceps");

        Assert.Empty(results);
    }
}
