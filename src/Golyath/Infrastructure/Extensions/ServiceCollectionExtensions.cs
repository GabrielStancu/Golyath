using Golyath.Core.Abstractions;
using Golyath.Infrastructure.Database;
using Golyath.Infrastructure.Repositories;

namespace Golyath.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Infrastructure services: database and repositories.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseService>();

        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IExerciseRepository, ExerciseRepository>();
        services.AddTransient<IWorkoutRepository, WorkoutRepository>();

        return services;
    }
}
