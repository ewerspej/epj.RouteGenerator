namespace RouteGeneratorSample.Cars;

public partial class AudiPage : ContentPage
{
    public AudiPage(AudiViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        SayHello();
    }
}