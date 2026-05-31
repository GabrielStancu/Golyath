using Golyath.Core.Abstractions;
using Golyath.Core.Entities;
using Golyath.Core.Enums;

namespace Golyath.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> HasCompletedOnboardingAsync()
    {
        var user = await _userRepository.GetCurrentUserAsync();
        return user is not null;
    }

    public Task<User?> GetCurrentUserAsync() => _userRepository.GetCurrentUserAsync();

    public async Task<User> CreateUserAsync(string nickname, DateTime birthday, double heightCm, double weightKg, Gender gender, FitnessGoal fitnessGoal, WeightUnit preferredUnit, AppLanguage language = AppLanguage.English)
    {
        var user = new User
        {
            Nickname = nickname,
            Birthday = birthday,
            HeightCm = heightCm,
            WeightKg = weightKg,
            Gender = gender,
            FitnessGoal = fitnessGoal,
            PreferredUnit = preferredUnit,
            Language = language,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.InsertAsync(user);
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
    }
}
