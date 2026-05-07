using Golyath.Core.Entities;

namespace Golyath.Core.Abstractions;

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name);
}
