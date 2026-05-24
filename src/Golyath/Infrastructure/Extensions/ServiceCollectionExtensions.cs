using Golyath.Application.Services;
using Golyath.Core.Abstractions;
using Golyath.Infrastructure.Database;
using Golyath.Infrastructure.Repositories;
using Golyath.Infrastructure.Services;
using Golyath.UI;
using Golyath.UI.ViewModels.Dashboard;
using Golyath.UI.ViewModels.Exercises;
using Golyath.UI.ViewModels.Analytics;
using Golyath.UI.ViewModels.Goals;
using Golyath.UI.ViewModels.History;
using Golyath.UI.ViewModels.Settings;
using Golyath.UI.ViewModels.Onboarding;
using Golyath.UI.ViewModels.Profile;
using Golyath.UI.ViewModels.Workout;
using Golyath.UI.Views.Dashboard;
using Golyath.UI.Views.Exercises;
using Golyath.UI.Views.Analytics;
using Golyath.UI.Views.Goals;
using Golyath.UI.Views.History;
using Golyath.UI.Views.Onboarding;
using Golyath.UI.Views.Profile;
using Golyath.UI.Views.Settings;
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
        services.AddTransient<ITagRepository, TagRepository>();
        services.AddTransient<IWorkoutTagRepository, WorkoutTagRepository>();
        services.AddTransient<IGoalRepository, GoalRepository>();
        services.AddTransient<IRoutineRepository, RoutineRepository>();
        services.AddTransient<IRoutineExerciseRepository, RoutineExerciseRepository>();

        // Application services
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IWorkoutService, WorkoutService>();
        services.AddTransient<ITagService, TagService>();
        services.AddTransient<IDashboardService, DashboardService>();
        services.AddTransient<IWorkoutHistoryService, WorkoutHistoryService>();
        services.AddTransient<IExerciseService, ExerciseService>();
        services.AddTransient<IAnalyticsService, AnalyticsService>();
        services.AddTransient<ISuggestionsService, SuggestionsService>();
        services.AddTransient<IGoalService, GoalService>();
        services.AddTransient<IPersonalRecordService, PersonalRecordService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<OnboardingDataService>();
        services.AddTransient<IExerciseSeederService, ExerciseSeederService>();
        services.AddTransient<IDataPortabilityService, DataPortabilityService>();
        services.AddTransient<IRoutineService, RoutineService>();

        // Shell (singleton so the same instance is reused when switching from onboarding)
        services.AddSingleton<AppShell>();
        services.AddTransient<MainPage>();

        // Dashboard
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<DashboardPage>();

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
        services.AddTransient<WorkoutTemplatesViewModel>();
        services.AddTransient<WorkoutTemplatesPage>();
        services.AddTransient<ActiveWorkoutViewModel>();
        services.AddTransient<ActiveWorkoutPage>();
        services.AddTransient<ExercisePickerViewModel>();
        services.AddTransient<ExercisePickerPage>();
        services.AddTransient<RoutineBuilderViewModel>();
        services.AddTransient<RoutineBuilderPage>();

        // Exercise library
        services.AddTransient<ExerciseLibraryViewModel>();
        services.AddTransient<ExerciseLibraryPage>();
        services.AddTransient<ExerciseDetailViewModel>();
        services.AddTransient<ExerciseDetailPage>();
        services.AddTransient<CreateExerciseViewModel>();
        services.AddTransient<CreateExercisePage>();

        // Workout history
        services.AddTransient<HistoryViewModel>();
        services.AddTransient<HistoryPage>();
        services.AddTransient<WorkoutDetailViewModel>();
        services.AddTransient<WorkoutDetailPage>();

        // Analytics
        services.AddTransient<AnalyticsViewModel>();
        services.AddTransient<AnalyticsPage>();


        // Goals
        services.AddTransient<GoalsViewModel>();
        services.AddTransient<GoalsPage>();
        services.AddTransient<AddGoalViewModel>();
        services.AddTransient<AddGoalPage>();

        // Settings
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SettingsPage>();

        return services;
    }
}
