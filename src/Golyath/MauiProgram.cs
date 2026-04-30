using Golyath.Infrastructure.Extensions;
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

        builder.Services.AddInfrastructure();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
