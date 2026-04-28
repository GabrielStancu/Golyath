using Golyath.Infrastructure;
using Golyath.Infrastructure.Data.Migrations;
using Golyath.Infrastructure.Data.Seeding;
using Microsoft.Extensions.Logging;

namespace Golyath;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "golyath.db");
        builder.Services.AddInfrastructure(dbPath);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        InitializeDatabaseAsync(app.Services).GetAwaiter().GetResult();

        return app;
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider services)
    {
        try
        {
            var migrator = services.GetRequiredService<DatabaseMigrator>();
            await migrator.MigrateAsync();

            var seeder = services.GetRequiredService<ExerciseSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Golyath] Database initialization failed: {ex}");
            throw;
        }
    }
}
