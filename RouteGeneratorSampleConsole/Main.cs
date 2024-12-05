using epj.RouteGenerator;
using RouteGeneratorSampleConsole.Route;
using System;

namespace RouteGeneratorSampleConsole
{
    [AutoRoutes("Route")]
    [ExtraRoute(nameof(Fastest))]
    [ExtraRoute("Inval!dRoute")] // invalid, will emit warning EXR001 and will be ignored
    [ExtraRoute("RouteWithAView", typeof(Fastest))]
    [ExtraRoute("RouteWithAView")] // duplicate, will emit warning EXR002 and will be ignored
    [ExtraRoute("RouteWithoutAType")] // no type available, will emit warning EXR003
    [ExtraRoute("RouteWithNull", null)] // no valid type available, will emit warning EXR003
    [ExtraRoute(null)] // will be ignored
    public static class Main
    {
        public static void PrintRoutes()
        {
            foreach (var route in Routes.RouteTypeMap)
            {
                if (route.Key.Equals(nameof(ShouldBeIgnoredRoute)))
                {
                    Console.WriteLine($"{nameof(ShouldBeIgnoredRoute)} should not be included in the route map");
                    break;
                }

                Console.WriteLine($"{route.Key}: {route.Value}");
            }
        }

    }
}
