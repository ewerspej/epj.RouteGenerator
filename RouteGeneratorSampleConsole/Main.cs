using epj.RouteGenerator;
using RouteGeneratorSampleConsole.Route;

namespace RouteGeneratorSampleConsole
{
    [AutoRoutes("Route")]
    [ExtraRoute(nameof(Fastest))]
    [ExtraRoute("Inval!dRoute")]
    [ExtraRoute("RouteWithoutAType")]
    [ExtraRoute("RouteWithAView", typeof(Fastest))]
    [ExtraRoute("RouteWithAView")]
    public static class Main
    {
        public static void PrintRoutes()
        {
            foreach (var route in Routes.RouteTypeMap)
            {
                Console.WriteLine($"{route.Key}: {route.Value}");
            }
        }
    }
}
