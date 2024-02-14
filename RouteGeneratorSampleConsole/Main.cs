using epj.RouteGenerator;
using RouteGeneratorSampleConsole.Route;

namespace RouteGeneratorSampleConsole
{
    [AutoRoutes("Route")]
    [ExtraRoute(nameof(Fastest))]
    [ExtraRoute("Inval!dRoute")]
    [ExtraRoute("RouteWithAView")]
    [ExtraRoute("RouteWithAView")]
    public static class Main
    {
        public static void PrintRoutes()
        {
            foreach (var route in Routes.RouteTypenames)
            {
                Console.WriteLine($"{route.Key}: {route.Value}");
            }
        }
    }
}
