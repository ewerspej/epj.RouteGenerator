using epj.RouteGenerator;
using RouteGeneratorSample.Cars;
using RouteGeneratorSample.Navigation;

namespace RouteGeneratorSample;

[AutoRoutes("Page")]
[ExtraRoute("SomeFaulty!Route")] // invalid, will emit warning EXR001 and will be ignored
[ExtraRoute("YetAnotherRoute", typeof(MainPage))]
[ExtraRoute("YetAnotherRoute")] // duplicate, will emit warning EXR002 and will be ignored
[ExtraRoute("SomeOtherRoute")] // valid, but no corresponding type available, will emit warning EXR003
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