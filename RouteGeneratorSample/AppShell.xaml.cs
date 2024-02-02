using RouteGeneratorSample.Cars;

namespace RouteGeneratorSample
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Routing.RegisterRoute(Routes.AudiPage, typeof(AudiPage));
            //Routing.RegisterRoute(Routes.VolvoPage, typeof(VolvoPage));
            Routing.RegisterRoute(Routes.SomeOtherRoute, typeof(MainPage));
            Routing.RegisterRoute(Routes.YetAnotherRoute, typeof(MainPage));

            //foreach (var route in Routes.AllRoutes)
            //{
            //    var type = Type.GetType(route);
            //    if (type is not null)
            //    {
            //        Routing.RegisterRoute(route, type);
            //    }
            //}

            foreach (var routeNamespace in Routes.RouteTypenames)
            {
                Routing.RegisterRoute(routeNamespace.Key, Type.GetType(routeNamespace.Value));
            }
        }
    }
}
