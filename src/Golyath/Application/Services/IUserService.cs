using Golyath.Core.Entities;
using Golyath.Core.Enums;

namespace Golyath.Application.Services;

public interface IUserService
{
    Task<bool> HasCompletedOnboardingAsync();
    Task<User?> GetCurrentUserAsync();
    Task<User> CreateUserAsync(string nickname, DateTime birthday, double heightCm, double weightKg, Gender gender, FitnessGoal fitnessGoal, WeightUnit preferredUnit, AppLanguage language = AppLanguage.English);
    Task UpdateUserAsync(User user);
}
