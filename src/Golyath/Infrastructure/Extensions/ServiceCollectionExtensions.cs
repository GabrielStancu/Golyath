using Golyath.Application.Services;
using Golyath.Core.Abstractions;
using Golyath.Infrastructure.Database;
using Golyath.Infrastructure.Repositories;
using Golyath.UI;
using Golyath.UI.ViewModels.Onboarding;
using Golyath.UI.ViewModels.Profile;
using Golyath.UI.ViewModels.Workout;
using Golyath.UI.Views.Onboarding;
using Golyath.UI.Views.Profile;
using Golyath.UI.Views.Workout;

namespace Golyath.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Infrastructure, Application, and UI services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Database
        services.AddSingleton<DatabaseService>();

        // Repositories
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IExerciseRepository, ExerciseRepository>();
        services.AddTransient<IWorkoutRepository, WorkoutRepository>();
        services.AddTransient<IWorkoutExerciseRepository, WorkoutExerciseRepository>();
        services.AddTransient<IWorkoutSetRepository, WorkoutSetRepository>();

        // Application services
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IWorkoutService, WorkoutService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<OnboardingDataService>();

        // Shell (singleton so the same instance is reused when switching from onboarding)
        services.AddSingleton<AppShell>();
        services.AddTransient<MainPage>();

        // Onboarding views + view models
        services.AddTransient<WelcomeViewModel>();
        services.AddTransient<ProfileSetupViewModel>();
        services.AddTransient<GoalSetupViewModel>();
        services.AddTransient<WelcomePage>();
        services.AddTransient<ProfileSetupPage>();
        services.AddTransient<GoalSetupPage>();
        services.AddTransient<OnboardingCompletePage>();

        // Profile edit
        services.AddTransient<EditProfileViewModel>();
        services.AddTransient<EditProfilePage>();

        // Workout logging
        services.AddTransient<ActiveWorkoutViewModel>();
        services.AddTransient<ActiveWorkoutPage>();
        services.AddTransient<ExercisePickerViewModel>();
        services.AddTransient<ExercisePickerPage>();

        return services;
    }
}
