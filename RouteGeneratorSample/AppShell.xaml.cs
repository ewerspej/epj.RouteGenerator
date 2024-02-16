using System.Diagnostics;

namespace RouteGeneratorSample
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(Routes.SomeOtherRoute, typeof(MainPage));

            foreach (var route in Routes.RouteTypeMap)
            {
                Routing.RegisterRoute(route.Key, route.Value);

                Debug.WriteLine($"{route.Key}: {route.Value}");
            }
        }
    }
}
