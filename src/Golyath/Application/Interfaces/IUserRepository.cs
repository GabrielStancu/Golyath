using Golyath.Core.Entities;

namespace Golyath.Application.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetCurrentUserAsync();
}
