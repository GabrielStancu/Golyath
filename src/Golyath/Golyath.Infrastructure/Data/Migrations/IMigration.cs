using SQLite;

namespace Golyath.Infrastructure.Data.Migrations;

public interface IMigration
{
    int Version { get; }
    Task UpAsync(SQLiteAsyncConnection db);
}
