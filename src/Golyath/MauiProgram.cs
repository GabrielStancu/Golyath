using Microsoft.Extensions.Logging;
using Golyath.Application.Interfaces;
using Golyath.Infrastructure.Database;
using Golyath.Infrastructure.Database.Migrations;
using Golyath.Infrastructure.Database.Repositories;

namespace Golyath
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp() => CreateMauiAppAsync().GetAwaiter().GetResult();

        private static async Task<MauiApp> CreateMauiAppAsync()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Infrastructure — Database
            builder.Services.AddSingleton(new DatabaseContext(Path.Combine(FileSystem.AppDataDirectory, "golyath.db3")));
            builder.Services.AddSingleton<IDatabaseMigrationRunner, DatabaseMigrationRunner>();

            // Repositories
            builder.Services.AddScoped<IUserRepository, SqliteUserRepository>();
            builder.Services.AddScoped<IExerciseRepository, SqliteExerciseRepository>();
            builder.Services.AddScoped<IWorkoutRepository, SqliteWorkoutRepository>();
            builder.Services.AddScoped<ISetRepository, SqliteSetRepository>();

            var app = builder.Build();

            var migrationRunner = app.Services.GetRequiredService<IDatabaseMigrationRunner>();
            await migrationRunner.MigrateAsync();

            return app;
        }
    }
}
