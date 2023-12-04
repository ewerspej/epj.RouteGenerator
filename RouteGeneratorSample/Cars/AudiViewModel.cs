using CommunityToolkit.Mvvm.Input;
using RouteGeneratorSample.Navigation;

namespace RouteGeneratorSample.Cars;

public class AudiViewModel(INavigationService navigationService)
{
    public IRelayCommand HomeCommand { get; } = new AsyncRelayCommand(async () => await navigationService.GoToAsync($"///{Routes.MainPage}"));

    public string Model => "S5";
    public int Year => 2020;
    public string Price => "From $55,000";
}