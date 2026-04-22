using CommunityToolkit.Maui;
using Golyath.Data;
using Golyath.Services;
using Golyath.ViewModels;
using Golyath.Views;
using Microsoft.Extensions.Logging;

namespace Golyath
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Database
            builder.Services.AddSingleton<GolyathDatabase>();

            // Services
            builder.Services.AddSingleton<IExerciseService, ExerciseService>();
            builder.Services.AddSingleton<IWorkoutService, WorkoutService>();
            builder.Services.AddSingleton<ITemplateService, TemplateService>();
            builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();

            // ViewModels
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<ExerciseListViewModel>();
            builder.Services.AddTransient<ExerciseDetailViewModel>();
            builder.Services.AddTransient<ActiveWorkoutViewModel>();
            builder.Services.AddTransient<HistoryViewModel>();
            builder.Services.AddTransient<SessionDetailViewModel>();
            builder.Services.AddTransient<AnalyticsViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<TemplateListViewModel>();
            builder.Services.AddTransient<TemplateEditViewModel>();

            // Pages
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<ExerciseListPage>();
            builder.Services.AddTransient<ExerciseDetailPage>();
            builder.Services.AddTransient<ActiveWorkoutPage>();
            builder.Services.AddTransient<HistoryPage>();
            builder.Services.AddTransient<SessionDetailPage>();
            builder.Services.AddTransient<AnalyticsPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<TemplateListPage>();
            builder.Services.AddTransient<TemplateEditPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
