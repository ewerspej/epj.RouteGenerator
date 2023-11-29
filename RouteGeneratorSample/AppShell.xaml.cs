using RouteGeneratorSample.Cars;

namespace RouteGeneratorSample
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("AudiPage", typeof(AudiPage));
            Routing.RegisterRoute("VolvoPage", typeof(VolvoPage));
        }
    }
}
