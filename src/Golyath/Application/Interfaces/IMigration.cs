using SQLite;

namespace Golyath.Application.Interfaces;

public interface IMigration
{
    int Version { get; }
    Task UpAsync(SQLiteAsyncConnection db);
}
