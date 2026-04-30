using Golyath.Core.Entities;

namespace Golyath.Core.Abstractions;

public interface IUserRepository : IRepository<User>
{
    /// <summary>Returns the single active user profile, or null if onboarding has not been completed.</summary>
    Task<User?> GetCurrentUserAsync();
}
