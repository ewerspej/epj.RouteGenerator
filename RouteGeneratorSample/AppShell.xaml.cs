using RouteGeneratorSample.Cars;

namespace RouteGeneratorSample
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(Routes.AudiPage, typeof(AudiPage));
            Routing.RegisterRoute(Routes.VolvoPage, typeof(VolvoPage));
            Routing.RegisterRoute(Routes.SomeOtherRoute, typeof(MainPage));
            Routing.RegisterRoute(Routes.YetAnotherRoute, typeof(MainPage));
        }
    }
}
