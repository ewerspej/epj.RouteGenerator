using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RouteGeneratorSample.Navigation;

namespace RouteGeneratorSample.Cars;

[QueryProperty(nameof(Owner), nameof(Owner))]
public partial class VolvoViewModel(INavigationService navigationService) : ObservableObject
{
    public IRelayCommand HomeCommand { get; } = new AsyncRelayCommand(async () => await navigationService.GoToAsync("///MainPage"));

    [ObservableProperty]
    private string _owner = string.Empty;

    public string Model => "XC90";
    public int Year => 2021;
    public string Price => "From $49,000";
}