using Golyath.Core.Entities;

namespace Golyath.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetCurrentUserAsync();
    Task<int> AddAsync(User user);
    Task UpdateAsync(User user);
}
