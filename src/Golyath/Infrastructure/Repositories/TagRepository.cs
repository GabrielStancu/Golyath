using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Infrastructure.Database;

namespace Golyath.Infrastructure.Repositories;

public sealed class TagRepository : BaseRepository<Tag>, ITagRepository
{
    public TagRepository(DatabaseService databaseService) : base(databaseService) { }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        var db = await GetDbAsync();
        return await db.Table<Tag>()
            .Where(t => t.Name == name)
            .FirstOrDefaultAsync();
    }
}
