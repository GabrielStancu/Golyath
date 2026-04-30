using SQLite;

namespace Golyath.Infrastructure.Database.Migrations;

public interface IMigration
{
    int Version { get; }
    Task ApplyAsync(SQLiteAsyncConnection db);
}
