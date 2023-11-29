namespace RouteGeneratorSample.Cars;

public partial class VolvoPage : ContentPage
{
    public VolvoPage(VolvoViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}