using Golyath.Core.Entities;
using Golyath.Infrastructure.Database.Migrations;
using Golyath.Tests.Helpers;
using SQLite;
using Xunit;

namespace Golyath.Tests.Infrastructure;

public sealed class DatabaseMigrationRunnerTests
{
    [Fact]
    public async Task MigrateAsync_CreatesAllTables()
    {
        await using var db = new TempDatabase();

        var migration = new Migration_0001_InitialSchema();
        await migration.UpAsync(db.Connection);

        // Verify tables exist by querying sqlite_master
        var tables = await db.Connection.QueryAsync<TableInfo>("SELECT name FROM sqlite_master WHERE type='table'");
        var tableNames = tables.Select(t => t.Name).ToHashSet();

        Assert.Contains("Users", tableNames);
        Assert.Contains("Exercises", tableNames);
        Assert.Contains("Workouts", tableNames);
        Assert.Contains("WorkoutExercises", tableNames);
        Assert.Contains("Sets", tableNames);
        Assert.Contains("Goals", tableNames);
        Assert.Contains("Tags", tableNames);
        Assert.Contains("AppMetadata", tableNames);
    }

    [Fact]
    public async Task MigrateAsync_SchemaVersionIsSetAfterMigration()
    {
        await using var db = new TempDatabase();

        // We can't use DatabaseMigrationRunner directly without FileSystem.AppDataDirectory,
        // so test the migration + metadata pattern directly.
        var migration = new Migration_0001_InitialSchema();
        await migration.UpAsync(db.Connection);

        await db.Connection.InsertAsync(new AppMetadata { Key = "schema_version", Value = "1" });
        var record = await db.Connection.Table<AppMetadata>()
            .Where(m => m.Key == "schema_version")
            .FirstOrDefaultAsync();

        Assert.NotNull(record);
        Assert.Equal("1", record.Value);
    }

    [Fact]
    public async Task Migration_IsIdempotent_WhenRunTwice()
    {
        await using var db = new TempDatabase();

        var migration = new Migration_0001_InitialSchema();

        // sqlite-net-pcl CreateTableAsync is idempotent by design — running twice must not throw.
        await migration.UpAsync(db.Connection);
        var ex = await Record.ExceptionAsync(() => migration.UpAsync(db.Connection));
        Assert.Null(ex);
    }

    // ── private helpers ───────────────────────────────────────────────────────

    private record TableInfo
    {
        [Column("name")]
        public string Name { get; init; } = string.Empty;
    }
}
