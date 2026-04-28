using Golyath.Core.Interfaces.Repositories;
using Golyath.Infrastructure.Data;
using Golyath.Infrastructure.Data.Migrations;
using Golyath.Infrastructure.Data.Repositories;
using Golyath.Infrastructure.Data.Seeding;
using Microsoft.Extensions.DependencyInjection;

namespace Golyath.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string databasePath)
    {
        services.AddSingleton(new AppDatabase(databasePath));

        services.AddTransient<IMigration, Migration_001_InitialSchema>();
        services.AddSingleton<DatabaseMigrator>();

        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IExerciseRepository, ExerciseRepository>();
        services.AddTransient<IWorkoutRepository, WorkoutRepository>();
        services.AddTransient<IWorkoutExerciseRepository, WorkoutExerciseRepository>();
        services.AddTransient<IWorkoutSetRepository, WorkoutSetRepository>();
        services.AddTransient<IGoalRepository, GoalRepository>();
        services.AddTransient<ITagRepository, TagRepository>();

        services.AddTransient<ExerciseSeeder>();

        return services;
    }
}
