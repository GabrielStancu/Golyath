using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Infrastructure.Database;

namespace Golyath.Infrastructure.Repositories;

public sealed class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(DatabaseService databaseService) : base(databaseService) { }

    public async Task<User?> GetCurrentUserAsync()
    {
        var db = await GetDbAsync();
        return await db.Table<User>().FirstOrDefaultAsync();
    }
}
