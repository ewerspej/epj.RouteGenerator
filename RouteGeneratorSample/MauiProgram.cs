using epj.RouteGenerator;
using Microsoft.Extensions.Logging;
using RouteGeneratorSample.Cars;
using RouteGeneratorSample.Navigation;

namespace RouteGeneratorSample
{
    [AutoRouteGeneration("Page")]
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

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<AudiPage>();
            builder.Services.AddTransient<AudiViewModel>();
            builder.Services.AddTransient<VolvoPage>();
            builder.Services.AddTransient<VolvoViewModel>();

            return builder.Build();
        }
    }
}
