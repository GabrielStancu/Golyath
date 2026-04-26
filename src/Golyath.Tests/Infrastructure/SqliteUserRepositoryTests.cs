using Golyath.Core.Entities;
using Golyath.Core.Enums;
using Golyath.Infrastructure.Database.Migrations;
using Golyath.Infrastructure.Database.Repositories;
using Golyath.Tests.Helpers;
using Xunit;

namespace Golyath.Tests.Infrastructure;

/// <remarks>
/// TESTABILITY GAP (flagged for Mickey):
/// <c>SqliteUserRepository</c> currently takes <c>DatabaseContext</c> in its constructor.
/// <c>DatabaseContext</c> resolves its SQLite path via <c>FileSystem.AppDataDirectory</c>
/// (a MAUI-only API), which is unavailable in a plain net9.0 test project.
///
/// Required fix — Mickey must choose one:
///   A) Add a secondary constructor: <c>SqliteUserRepository(SQLiteAsyncConnection conn)</c>
///   B) Extract an internal <c>DatabaseContext(string path)</c> overload that skips MAUI APIs.
///
/// The tests below are written assuming Option A is implemented.  If that constructor is
/// absent, these tests will fail at compile time with a missing overload error — not a
/// runtime error — making the gap immediately visible.
/// </remarks>
public sealed class SqliteUserRepositoryTests : IAsyncLifetime
{
    private TempDatabase _db = null!;
    private SqliteUserRepository _repository = null!;

    public async Task InitializeAsync()
    {
        _db = new TempDatabase();
        var migration = new Migration_0001_InitialSchema();
        await migration.UpAsync(_db.Connection);
        _repository = new SqliteUserRepository(_db.Connection);
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
    }

    // ── InsertAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task InsertAsync_PersistsUser()
    {
        var user = new User
        {
            Nickname = "Gabriel",
            Birthday = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Height = 180.0,
            Weight = 80.0,
            Gender = Gender.Male,
            FitnessGoal = FitnessGoal.Strength,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _repository.InsertAsync(user);

        Assert.True(id > 0);
    }

    // ── GetCurrentUserAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetCurrentUserAsync_ReturnsInsertedUser()
    {
        var user = new User { Nickname = "TestUser", CreatedAt = DateTime.UtcNow };
        await _repository.InsertAsync(user);

        var retrieved = await _repository.GetCurrentUserAsync();

        Assert.NotNull(retrieved);
        Assert.Equal("TestUser", retrieved.Nickname);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var user = new User { Nickname = "Before", CreatedAt = DateTime.UtcNow };
        await _repository.InsertAsync(user);

        var saved = await _repository.GetCurrentUserAsync();
        Assert.NotNull(saved);
        saved.Nickname = "After";
        await _repository.UpdateAsync(saved);

        var updated = await _repository.GetCurrentUserAsync();
        Assert.Equal("After", updated!.Nickname);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_RemovesUser()
    {
        var user = new User { Nickname = "ToDelete", CreatedAt = DateTime.UtcNow };
        await _repository.InsertAsync(user);

        var saved = await _repository.GetCurrentUserAsync();
        Assert.NotNull(saved);
        await _repository.DeleteAsync(saved.Id);

        var allUsers = await _repository.GetAllAsync();
        Assert.Empty(allUsers);
    }
}
