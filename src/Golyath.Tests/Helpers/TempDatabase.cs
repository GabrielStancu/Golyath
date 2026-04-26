using SQLite;

namespace Golyath.Tests.Helpers;

/// <summary>
/// Integration test helper — creates an isolated, temporary file-based SQLite database per test.
/// The database file is deleted automatically when the test completes.
/// </summary>
/// <remarks>
/// We use a raw <see cref="SQLiteAsyncConnection"/> rather than going through
/// <c>DatabaseContext</c>, because <c>DatabaseContext</c> resolves the DB path via
/// <c>FileSystem.AppDataDirectory</c> — a MAUI-specific API unavailable in a plain
/// <c>net9.0</c> test project.  Mickey needs to make <c>DatabaseContext</c> accept an
/// optional path string (or expose the inner connection) so that repositories can also
/// be constructed directly with a connection for testability.
/// See: decisions/inbox/duke-testability-gap.md
/// </remarks>
public sealed class TempDatabase : IAsyncDisposable
{
    private readonly string _path;

    public SQLiteAsyncConnection Connection { get; }

    public TempDatabase()
    {
        _path = Path.Combine(Path.GetTempPath(), $"golyath_test_{Guid.NewGuid():N}.db3");
        Connection = new SQLiteAsyncConnection(_path);
    }

    public async ValueTask DisposeAsync()
    {
        await Connection.CloseAsync();
        if (File.Exists(_path))
            File.Delete(_path);
    }
}
