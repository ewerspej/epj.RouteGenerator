using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RouteGeneratorSample.Navigation;

namespace RouteGeneratorSample;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [RelayCommand]
    private async Task GoToAudiPageAsync()
    {
        await _navigationService.GoToAsync($"/{Routes.AudiPage}");
    }

    [RelayCommand]
    private async Task GoToVolvoPageAsync(string owner)
    {
        await _navigationService.GoToAsync($"/{Routes.VolvoPage}?Owner={owner}");
    }

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
}