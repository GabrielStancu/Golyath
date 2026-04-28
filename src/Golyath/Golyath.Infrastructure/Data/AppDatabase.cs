using SQLite;

namespace Golyath.Infrastructure.Data;

public class AppDatabase
{
    public SQLiteAsyncConnection Connection { get; }

    public AppDatabase(string databasePath)
    {
        Connection = new SQLiteAsyncConnection(
            databasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
    }
}
