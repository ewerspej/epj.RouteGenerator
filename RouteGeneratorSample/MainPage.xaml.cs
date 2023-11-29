using RouteGeneratorSample.Navigation;

namespace RouteGeneratorSample
{
    public partial class MainPage : ContentPage
    {
        private readonly INavigationService _navigationService;

        public MainPage(INavigationService navigationService)
        {
            _navigationService = navigationService;

            InitializeComponent();
        }

        private async void OnAudiButtonClicked(object? sender, EventArgs e)
        {
            await _navigationService.GoToAsync($"/{Routes.AudiPage}");
        }

        private async void OnVolvoButtonClicked(object? sender, EventArgs e)
        {
            await _navigationService.GoToAsync($"/{Routes.VolvoPage}?Owner=Julian");
        }
    }
}
