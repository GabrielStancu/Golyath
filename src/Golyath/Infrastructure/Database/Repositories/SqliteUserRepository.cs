using SQLite;
using Golyath.Application.Interfaces;
using Golyath.Core.Entities;

namespace Golyath.Infrastructure.Database.Repositories;

public sealed class SqliteUserRepository : SqliteRepository<User>, IUserRepository
{
    public SqliteUserRepository(DatabaseContext context) : base(context) { }

    public SqliteUserRepository(SQLiteAsyncConnection connection) : base(connection) { }

    public async Task<User?> GetCurrentUserAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<User>().FirstOrDefaultAsync();
    }
}
